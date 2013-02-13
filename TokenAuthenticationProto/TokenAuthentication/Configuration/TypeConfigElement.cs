using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.ComponentModel;

namespace TokenAuthentication.Web.Configuration
{
	/// <summary>
	/// A configuration element that allows you to configure a managed type.
	/// </summary>
	public sealed class TypeConfigElement : ConfigurationElement
	{
		/// <summary>
		/// Creates a new instance of the class.
		/// </summary>
		public TypeConfigElement() { }

		internal TypeConfigElement(Type type)
		{
			base["type"] = type;
		}

		/// <summary>
		/// Returns a collection containing key-value pairs that specify settings
		/// for an instance of the type specified in the <see cref="Type"/> property.
		/// </summary>
		[ConfigurationProperty("settings", IsRequired = false)]
		public KeyValueConfigurationCollection Settings
		{
			get { return (KeyValueConfigurationCollection)base["settings"]; }
		}

		/// <summary>
		/// Specifies the managed type.
		/// </summary>
		[ConfigurationProperty("type", IsRequired = true)]
		[TypeConverter(typeof(TypeNameConverter))]
		public Type Type
		{
			get { return (Type)base["type"]; }
		}

		/// <summary>
		/// Creates an instance of the type specified in the <see cref="Type"/>
		/// property. If that instance implements the <see cref="ISettingsContainer"/>
		/// interface, the settings from the <see cref="Settings"/> collection
		/// will be copied to the <see cref="ISettingsContainer.Settings"/>
		/// collection of the generated type.
		/// </summary>
		/// <typeparam name="T">The type to return the created instance as.</typeparam>
		public T CreateInstance<T>()
		{
			if (null == this.Type)
			{
				return default(T);
			}

			var instance = (T)Activator.CreateInstance(this.Type);

			if (instance is ISettingsContainer)
			{
				ISettingsContainer settingsInstance = (ISettingsContainer)instance;
				foreach (var key in this.Settings.AllKeys)
				{
					settingsInstance.Settings.Add(key, this.Settings[key].Value);
				}
			}

			return instance;
		}

	}
}
