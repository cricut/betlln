using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Betlln.Data.Integration.Core;
using Newtonsoft.Json;

namespace Betlln.Data.Integration.Json
{
    internal abstract class JsonDocumentCollection : IDataRecordIterator
    {
        protected ResourceStack _readPipeline;
        private StreamWriter _debugWriter;

        protected JsonDocumentCollection()
        {
            _readPipeline = new ResourceStack();
        }
        
        protected abstract string SourceObjectName { get; }
        private ulong Position { get; set; }
        
        private void Initialize()
        {
            PopulateReadPipeline();

            Stream finalStream = _readPipeline.Tip as Stream;
            Debug.Assert(finalStream != null);
            StreamReader reader = new StreamReader(finalStream);
            _readPipeline.Push(reader);
            Position = 0;

            if (Debugger.IsAttached && !SystemVariables.Multithreaded)
            {
                try
                {
                    string debugFilePath = Path.Combine(Path.GetTempPath(), "JSON_DEBUG.TXT");
                    _debugWriter = new StreamWriter(System.IO.File.Open(debugFilePath, FileMode.Create, FileAccess.Write, FileShare.Read));
                }
                catch (Exception ioException)
                {
                    Dts.Events.RaiseInformation($"Could not create debug log.{Environment.NewLine}{ioException}");
                }
            }
        }

        protected abstract void PopulateReadPipeline();

        public IEnumerator<DataRecord> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool MoveNext()
        {
            if (_readPipeline == null)
            {
                Current = null;
                return false;
            }

            if (_readPipeline.Count == 0)
            {
                Initialize();
            }

            StreamReader reader = _readPipeline.Tip as StreamReader;
            if (reader == null)
            {
                throw new JsonReaderException("No data to read.");
            }

            bool done = false;
            int depth = -1;
            char? currentQuoteDelimiter = null;
            StringBuilder buffer = new StringBuilder();

            char currentCharacter = '\0';
            while (!done)
            {
                char previousCharacter = currentCharacter;
                currentCharacter = (char) reader.Read();
                Position++;

                if (!currentQuoteDelimiter.HasValue && IsQuoteDelimiter(currentCharacter))
                {
                    currentQuoteDelimiter = currentCharacter;
                }
                else if (currentQuoteDelimiter.HasValue)
                {
                    if (currentCharacter == currentQuoteDelimiter.Value && previousCharacter != '\\')
                    {
                        currentQuoteDelimiter = null;
                    }
                }

                if (!currentQuoteDelimiter.HasValue)
                {
                    if (currentCharacter == '{')
                    {
                        if (depth == -1)
                        {
                            depth = 0;
                        }
                        depth++;
                    }
                    else if(currentCharacter == '}')
                    {
                        depth--;
                    }

                    if (currentCharacter == '>')
                    {
                        throw new FormatException($"{CurrentErrorMessage}: The character '>' is not valid in JSON.");
                    }
                }

                buffer.Append(currentCharacter);

                if (depth == 0 || reader.EndOfStream)
                {
                    string content = buffer.ToString();
                    WriteToDebugLog(content);

                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        try
                        {
                            Current = new JsonDocument(content).ToRecord();
                        }
                        catch (Exception parseError)
                        {
                            throw new Exception(CurrentErrorMessage, parseError);
                        }
                    }
                    else
                    {
                        Debug.Assert(reader.EndOfStream);
                        Dispose();
                        return false;
                    }
                        
                    buffer.Clear();
                    done = true;
                }
            }

            if (reader.EndOfStream)
            {
                Dispose();
            }

            return true;
        }

        private string CurrentErrorMessage
        {
            get { return $"Read error in {SourceObjectName} at {Position}"; }
        }

        private static bool IsQuoteDelimiter(char character)
        {
            return character == '\'' || character == '"';
        }

        private void WriteToDebugLog(string content)
        {
            if (_debugWriter != null)
            {
                _debugWriter.Write(content);
                _debugWriter.WriteLine();
                _debugWriter.WriteLine();
                _debugWriter.Flush();
            }
        }

        public void Reset()
        {
            Dispose();
            Initialize();
        }

        public DataRecord Current { get; private set; }
        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _debugWriter?.Dispose();
            _readPipeline?.Dispose();
            _readPipeline = null;
            _debugWriter = null;
        }
    }
}