using System;
using System.Reflection;

namespace Betlln.Data.Integration.Core
{
    internal class PropertyBinder : Binder
    {
        private PropertyBinder(object target, PropertyInfo propertyInfo) 
            : base(target, propertyInfo.Name, propertyInfo.PropertyType.GetProperties())
        {
        }

        public static PropertyBinder AttachTo(object target, string propertyName)
        {
            if (target == null)
            {
                throw new ArgumentNullException();
            }

            PropertyInfo propertyInfo = target.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
            {
                return null;
            }

            object propertyInstance = Activator.CreateInstance(propertyInfo.PropertyType);
            propertyInfo.SetValue(target, propertyInstance);

            return new PropertyBinder(propertyInstance, propertyInfo);
        }
    }
}