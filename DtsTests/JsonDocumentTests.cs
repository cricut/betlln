using System;
using System.IO;
using System.Linq;
using Betlln.Data.Integration.Json;
using DtsTests.Properties;
using NUnit.Framework;

namespace DtsTests
{
    [TestFixture]
    public class JsonDocumentTests
    {
        [Test]
        public void InvalidFileThrowsException()
        {
            Assert.Throws<FormatException>(() =>
            {
                using (ResourceReader tester = new ResourceReader(Resources.BadEncodingFileExample))
                {
                    int actualCount = tester.Count();
                    Assert.AreEqual(3201, actualCount);
                }
            });
        }

        [Test]
        public void SimpleArrayPropertiesTest()
        {
            string rawContent = GetRawContent();

            JsonDocument document = new JsonDocument(rawContent);

            AssertProperty(document, "source.action.data.imageIDs", "[3577988,0,0,0,0,0,0,0,0,0,0,0,3577988,0,0,0,0,0,0,0,0,0,0,0]");
        }

        [Test]
        public void ComplexArrayPropertiesTest()
        {
            string rawContent = GetRawContent();

            JsonDocument document = new JsonDocument(rawContent);

            Assert.NotNull(document.Properties.Find(x => x.Name == "source.action.data.machineInfo.Firmware.UpdateOptions"));
        }

        [Test]
        public void FlatPropertiesTest()
        {
            string rawContent = GetRawContent();

            JsonDocument document = new JsonDocument(rawContent);

            AssertProperty(document, "source.action.name", "FirstEventName");
            AssertProperty(document, "source.action.context", "ThisIsAContext");
            AssertProperty(document, "source.action.data.mainID", "aa2e3272-e949-4bfe-bd85-0519af5f84b8");
            AssertProperty(document, "source.action.data.referenceID", "70310977");
            AssertProperty(document, "source.action.data.objectID", "70310977");
            AssertProperty(document, "source.action.data.mainIndex", "0");
            AssertProperty(document, "source.action.data.mainColor", "#464646");
            AssertProperty(document, "source.action.data.mainWidthIn", "12");
            AssertProperty(document, "source.action.data.mainHeightIn", "12");
            AssertProperty(document, "source.action.data.secondaryWidthPt", "864");
            AssertProperty(document, "source.action.data.secondaryHeightPt", "864");
            AssertProperty(document, "source.action.data.secondaryAreaPt", "746496");
            AssertProperty(document, "source.action.data.secondaryLengthPt", null);
            AssertProperty(document, "source.action.data.machineInfo.BlueTooth", "False");
            AssertProperty(document, "source.action.data.machineInfo.BlueToothDeviceID", "");
            AssertProperty(document, "source.action.data.machineInfo.BootLoader", "False");
            AssertProperty(document, "source.action.data.machineInfo.DeviceID", "1");
            AssertProperty(document, "source.action.data.machineInfo.DeviceType", "DeviceCodeName");
            AssertProperty(document, "source.action.data.machineInfo.Firmware.Version.ButtonBoard", "0");
            AssertProperty(document, "source.action.data.machineInfo.Firmware.Version.FirmwareHi", "0");
            AssertProperty(document, "source.action.data.machineInfo.Firmware.Version.FirmwareLo", "3091");
            AssertProperty(document, "source.action.data.machineInfo.Firmware.Version.Hardware", "3");
            AssertProperty(document, "source.action.data.machineInfo.HID", "False");
            AssertProperty(document, "source.action.data.machineInfo.IsOpened", "True");
            AssertProperty(document, "source.action.data.machineInfo.MachineData.MachineID", "208598");
            AssertProperty(document, "source.action.data.machineInfo.MachineData.bluetoothVersion", "1");
            AssertProperty(document, "source.action.data.machineInfo.MachineData.bypassIPCheck", "False");
            AssertProperty(document, "source.action.data.machineInfo.MachineData.calibrationValuesStored", "False");
            AssertProperty(document, "source.action.data.machineInfo.MachineData.companyID", "14");
            AssertProperty(document, "source.action.data.machineInfo.MachineData.companyName", "AMAZON.COM");
            AssertProperty(document, "source.action.data.machineInfo.MachineData.duration", "14");
            AssertProperty(document, "source.action.data.machineInfo.MachineData.durationType", "Day");
            AssertProperty(document, "source.action.data.machineInfo.MachineData.durationTypeID", "1");
            AssertProperty(document, "source.action.data.machineInfo.MachineData.Description", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.");
            AssertProperty(document, "source.action.data.machineInfo.MachineData.Url", "https://d2e2oszluhwxlw.cloudfront.net/thing/pdfs/content/B-Included-with-machine-Links.pdf");
            AssertProperty(document, "source.action.data.machineInfo.MachineData.firmwareValuesStored", "3091");
            AssertProperty(document, "source.action.data.machineInfo.MachineData.machineType", "TypeName");
            AssertProperty(document, "source.action.data.machineInfo.MachineData.machineTypeID", "8");
            AssertProperty(document, "source.action.data.machineInfo.MachineData.primaryUserSet", "True");
            AssertProperty(document, "source.action.data.machineInfo.MachineName", "ProductName : 5 (USB)");
            AssertProperty(document, "source.action.data.machineInfo.Port", "5");
            AssertProperty(document, "source.action.data.machineInfo.Serial", "SF6JW46U2W46H2W456U");
            AssertProperty(document, "source.action.data.buttonPosition", "1");
            AssertProperty(document, "source.action.data.sourceGUID", "82");
            AssertProperty(document, "source.action.data.sourceID", "59567a3319b49911c30e5d8b");
            AssertProperty(document, "source.action.data.isDraftMode", "False");
            AssertProperty(document, "source.action.data.openedType", "none");
            AssertProperty(document, "source.action.data.startedSource", "CampFire");
            AssertProperty(document, "source.app.name", "AppName");
            AssertProperty(document, "source.app.environment", "app.example.com");
            AssertProperty(document, "user.device.platform", "Win32");
            AssertProperty(document, "user.device.language", "en-US");
            AssertProperty(document, "user.device.userAgent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            AssertProperty(document, "user.device.model", "Chrome");
            AssertProperty(document, "user.device.bowser", "Chrome");
            AssertProperty(document, "user.device.version", "61.0");
            AssertProperty(document, "user.userID", "1344763");
            AssertProperty(document, "status.startedOn", DateTime.Parse("10/4/2017 8:33:02 PM").ToString());
            AssertProperty(document, "status.completedOn", DateTime.Parse("10/4/2017 8:33:02 PM").ToString());
        }

        private static string GetRawContent()
        {
            string rawContent;

            using (MemoryStream fileContents = new MemoryStream(Resources.SingleExample))
            {
                using (StreamReader reader = new StreamReader(fileContents))
                {
                    rawContent = reader.ReadToEnd();
                }
            }

            return rawContent;
        }

        private static void AssertProperty(JsonDocument actualDocument, string expectedName, string expectedValue)
        {
            JsonProperty property = actualDocument.Properties.Find(x => x.Name == expectedName);
            Assert.NotNull(property);
            Assert.AreEqual(expectedValue, property.Value);
        }

        private class ResourceReader : JsonDocumentCollection
        {
            private readonly byte[] _resource;

            public ResourceReader(byte[] resource)
            {
                _resource = resource;
            }

            protected override string SourceObjectName
            {
                get { return "__test__"; }
            }

            protected override void PopulateReadPipeline()
            {
                MemoryStream ms = new MemoryStream(_resource);
                ms.Position = 0;
                _readPipeline.Push(ms);
            }
        }
    }
}