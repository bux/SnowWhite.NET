

namespace System.Runtime.Serialization.Plists
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using System.Xml.XPath;

    /// <summary>
    /// Will create a valid-formatted Property List, as described by Apple.
    /// 
    /// This is most efficient way (when being processed on the iPhone) of returning
    /// data from a web service to the iPhone.
    /// 
    /// This class cannot be instantiated, or sub-classed.
    /// </summary>
    public sealed class PlistXmlDocument
    {
        // Prevent instantiation
        private PlistXmlDocument()
        {
        }


        public static object CreateObjectFromPlist(string plist)
        {
            object newObject = null;

            if (!string.IsNullOrEmpty(plist))
            {
                try
                {
                    // Create XmlDocument
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(plist);

                    XmlNode topNode = xml.SelectSingleNode("/plist");

                    try
                    {
                        XmlNode node = xml.SelectSingleNode("dict/key[.='className']");

                        if (node != null)
                        {
                            // This node MUST be in the dictionary if an instance of a class is to 
                            // be re-created from the plist.  The class also requires a constructor
                            // that takes no parameters.
                            string keyName = node.InnerText;
                            string typeString = node.NextSibling.InnerText;
                            Type newType = Type.GetType(typeString);
                            ConstructorInfo conInfo = newType.GetConstructor(Type.EmptyTypes);
                            newObject = conInfo.Invoke(null);

                            XmlNode dataNode = xml.SelectSingleNode("dict/key[.='classData']");

                            if (dataNode != null)
                            {
                                XmlNodeList items = dataNode.NextSibling.SelectNodes("key");

                                foreach (XmlNode keyNode in items)
                                {
                                    processValueTag(newObject, newType, keyNode);
                                }
                            }
                        }
                    }
                    catch (XPathException)
                    {
                    }

                    newObject = createObjectFromXmlFragment(newObject, topNode);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return newObject;
        }


        private static object createObjectFromXmlFragment(object newObject, XmlNode xml)
        {
            return newObject;
        }

        private static void processValueTag(object newObject, Type newType, XmlNode keyNode)
        {
            XmlNode valueNode = keyNode.NextSibling;
            string plistType = valueNode.Name;
            string key = keyNode.InnerText;
            string value = valueNode.InnerText;
            PropertyInfo property = newType.GetProperty(key);

            switch (plistType)
            {
                case "array":
                    MethodInfo theMethod = newType.GetMethod("ToArray");

                    object array = theMethod.Invoke(value, null);
                    object[] arr = array as object[];

                    break;
                case "dict":
                    break;
                case "integer":
                    switch (property.PropertyType.FullName)
                    {
                        case "System.Int16":
                            Int16 int16Value = Convert.ToInt16(value);
                            property.SetValue(newObject, int16Value, null);
                            break;
                        case "System.Int32":
                            Int32 int32Value = Convert.ToInt32(value);
                            property.SetValue(newObject, int32Value, null);
                            break;
                        case "System.Int64":
                            Int64 int64Value = Convert.ToInt64(value);
                            property.SetValue(newObject, int64Value, null);
                            break;
                    }
                    break;
                case "real":
                    switch (property.PropertyType.FullName)
                    {
                        case "System.Single":
                            break;
                        case "System.Double":
                            break;
                        case "System.Decimal":
                            break;
                    }
                    break;
                case "string":
                    property.SetValue(newObject, value, null);
                    break;
                case "true":
                    property.SetValue(newObject, true, null);
                    break;
                case "false":
                    property.SetValue(newObject, false, null);
                    break;
            }
        }

        /// <summary>
        /// Creates an instance of an object from a specifically crafted XML plist.
        /// </summary>
        /// <param name="plist">XML Property List.</param>
        /// <param name="inputObject">An instance of the class to be populated.</param>
        /// <returns>Instance of specified class with values from the plist.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.Xml.XmlException"></exception>
        public static object CreateObjectFromPlist(string plist, object inputObject)
        {
            if (inputObject != null)
            {
                if (!string.IsNullOrEmpty(plist))
                {
                    try
                    {
                        // Create XmlDocument
                        XmlDocument xml = new XmlDocument();
                        xml.LoadXml(plist);

                        try
                        {
                            XmlNode topNode = xml.SelectSingleNode("/plist/dict/key[.='className']");

                            if (topNode != null)
                            {
                                string keyName = topNode.InnerText;

                                string expectedType = topNode.NextSibling.InnerText;
                                string actualType = inputObject.GetType().ToString();

                                if (expectedType.Equals(actualType))
                                {
                                    Type objectType = inputObject.GetType();
                                    XmlNode dataNode = xml.SelectSingleNode("/plist/dict/key[.='classData']");

                                    if (dataNode != null)
                                    {
                                        XmlNodeList items = dataNode.NextSibling.SelectNodes("key");

                                        foreach (XmlNode keyNode in items)
                                        {
                                            XmlNode valueNode = keyNode.NextSibling;
                                            string plistType = valueNode.Name;
                                            string key = keyNode.InnerText;
                                            string value = valueNode.InnerText;
                                            PropertyInfo property = objectType.GetProperty(key);

                                            switch (plistType)
                                            {
                                                case "integer":
                                                    switch (property.PropertyType.FullName)
                                                    {
                                                        case "System.Int16":
                                                            Int16 int16Value = Convert.ToInt16(value);
                                                            property.SetValue(inputObject, int16Value, null);
                                                            break;
                                                        case "System.Int32":
                                                            Int32 int32Value = Convert.ToInt32(value);
                                                            property.SetValue(inputObject, int32Value, null);
                                                            break;
                                                        case "System.Int64":
                                                            Int64 int64Value = Convert.ToInt64(value);
                                                            property.SetValue(inputObject, int64Value, null);
                                                            break;
                                                    }
                                                    break;
                                                case "real":
                                                    switch (property.PropertyType.FullName)
                                                    {
                                                        case "System.Single":
                                                            break;
                                                        case "System.Double":
                                                            break;
                                                        case "System.Decimal":
                                                            break;
                                                    }
                                                    break;
                                                case "string":
                                                    property.SetValue(inputObject, value, null);
                                                    break;
                                                case "true":
                                                    property.SetValue(inputObject, true, null);
                                                    break;
                                                case "false":
                                                    property.SetValue(inputObject, false, null);
                                                    break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    throw new ArgumentException("Object instance Type differs from that defined in plist", "inputObject");
                                }
                            }
                        }
                        catch (XPathException)
                        {
                        }
                    }
                    catch (XmlException)
                    {
                        // The plist isn't valid Xml (therefore isn't a valid plist).
                        // So just pass the input object out, unchanged.
                        return inputObject;
                    }
                }
            }
            else
            {
                throw new ArgumentNullException("inputObject");
            }

            return inputObject;
        }

        /// <summary>
        /// Creates a string plist representation of the specified object, which can
        /// be turned into an xml document if required.
        /// </summary>
        /// <param name="value">Object to turn into plist</param>
        /// <returns>String Plist.</returns>
        public static string CreateDocument(object value)
        {
            TextWriter xml = new StringWriterWithEncoding(new StringBuilder(), Encoding.UTF8);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.NewLineChars = "\r\n";
            settings.IndentChars = "\t";
            settings.Indent = true;
            settings.NewLineHandling = NewLineHandling.Replace;
            XmlWriter writer = XmlTextWriter.Create(xml, settings);

            writer.WriteStartDocument();
            writer.WriteDocType("plist", "-//Apple//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd",null);

            
            writer.WriteStartElement("plist");
            writer.WriteAttributeString("version", "1.0");

            createDocumentFragment(writer, value,true);

            writer.WriteEndElement();
            writer.Close();

            return xml.ToString();
        }

        /// <summary>
        /// Creates a valid XML tag pair in the XmlWriter, based upon the object passed in.
        /// </summary>
        /// <param name="writer">The XmlWriter to add the XML Tag.</param>
        /// <param name="value">An object to represent in the PropertyList.</param>
        private static void createDocumentFragment(XmlWriter writer, object value, bool isTopLevel)
        {
            Type objectType = value.GetType();
            Type baseType = objectType.BaseType;
            string objType = value.GetType().FullName;
            string basType = baseType.FullName;

            // An array isn't of a particular type, but needs to be handled separately.
            if (objectType.IsArray)
            {
                objType = "Dummy.Namespace.Array";
            }

            // A dictionary is processed as a series of key value pairs.
            if (
                (objectType.IsGenericType && objType.Contains("Generic.Dictionary")) ||
                (baseType.IsGenericType && basType.Contains("Generic.Dictionary"))
                )
            {
                Dictionary<string, object> dict = createObjectDictionary(value, objectType);

                if (!isTopLevel)
                    writer.WriteStartElement("array");
                writer.WriteStartElement("dict");

                // Outputs a key, then value, for each item in the dictionary.
                foreach (KeyValuePair<string, object> item in dict)
                {
                    writer.WriteElementString("key", item.Key);
                    createDocumentFragment(writer, item.Value,false);
                }

                writer.WriteEndElement();      // 'dict' element
                if (!isTopLevel)
                    writer.WriteEndElement();      // 'array' element
            }
            else
            {
                // Processing for some standard 'C' wrappers and DataSet/DataTable/DataRows.
                createDocumentFragmentForNonDictionary(writer, value, objectType, objType);
            }
        }

        private static void createDocumentFragmentForNonDictionary(XmlWriter writer, object value, Type objectType, string objType)
        {
            Type baseType = objectType.BaseType;
            string basType = baseType.FullName;

            string stringValue = "";

            if (objType.Equals("System.String"))
            {
                stringValue = value.ToString();
                writer.WriteElementString("string", stringValue);
            }
            else if (objType.Contains("System.Int"))
            {
                stringValue = value.ToString();
                writer.WriteElementString("integer", stringValue);
            }
            else if (objType.Equals("System.Boolean"))
            {
                bool boolValue = Convert.ToBoolean(value);
                writer.WriteElementString((boolValue ? "true" : "false"), null);
            }
            else if (objType.Equals("System.Single") ||
               objType.Equals("System.Double") ||
               objType.Equals("System.Decimal"))
            {
                stringValue = Convert.ToString(value,System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                writer.WriteElementString("real", stringValue);
            }
            else if (objType.Equals("System.DateTime"))
            {
                DateTime date = Convert.ToDateTime(value);
                string dateValue, timeValue;

                dateValue = date.ToString("yyyy-MM-dd");
                timeValue = date.ToString("HH:mm:ss");

                stringValue = string.Format("{0}T{1}Z", dateValue, timeValue);
                writer.WriteElementString("date", stringValue);
            }
            else if (objType.Equals("System.Data.DataSet") ||
           objType.Equals("System.Data.DataTable") ||
           objType.Equals("System.Data.DataRow"))
            {
                createDocumentFragmentFromDataObject(writer, value);
            }
            else if (objType.Equals("Dummy.Namespace.Array"))
            {
                object[] arr = value as object[];
                writer.WriteStartElement("array");

                foreach (object obj in arr)
                {
                    createDocumentFragment(writer, obj,false);
                }

                writer.WriteEndElement();       // 'array' tag
            }
            else if (
                (objectType.IsGenericType && objType.Contains("Generic.List")) ||
                (baseType.IsGenericType && basType.Contains("Generic.List"))
                )
            {
                MethodInfo theMethod = objectType.GetMethod("ToArray");

                object array = theMethod.Invoke(value, null);
                object[] arr = array as object[];

                if (arr != null)
                {
                    writer.WriteStartElement("array");

                    foreach (object obj in arr)
                    {
                        createDocumentFragment(writer, obj,false);
                    }

                    writer.WriteEndElement();       // 'array' tag
                }
            }
            else
            {
                if (objectType.IsSerializable)
                {
                    // If the object is serializable, retrieves the public properties
                    // and creates a 'dict' element containing all of them.
                    // The class does NOT have to be marked as Serializable for this to work.
                    System.Reflection.PropertyInfo[] propertyInfo = objectType.GetProperties();

                    writer.WriteStartElement("dict");

                    // Outputs a key then value for each public property.
                    // The order of output is the same as their definition in source code.
                    for (int index = 0; index < propertyInfo.Length; index++)
                    {
                        writer.WriteElementString("key", propertyInfo[index].Name);
                        createDocumentFragment(writer, propertyInfo[index].GetValue(value, null), false);
                    }

                    writer.WriteEndElement();       // 'dict' tag
                }
            }
        }

        private static Dictionary<string, object> createObjectDictionary(object value, Type objectType)
        {
            PropertyInfo keysProperty = objectType.GetProperty("Keys");
            PropertyInfo valuesProperty = objectType.GetProperty("Values");

            object objKeys = keysProperty.GetValue(value, null);
            object objValues = valuesProperty.GetValue(value, null);

            Dictionary<string, object> dict = new Dictionary<string, object>();

            ICollection keyCollection = objKeys as ICollection;
            int count = keyCollection.Count;

            string[] keyArray = new string[count];
            object[] valueArray = new object[count];

            keyCollection.CopyTo(keyArray, 0);
            ((ICollection)objValues).CopyTo(valueArray, 0);

            for (int index = 0; index < keyArray.Length; index++)
            {
                dict.Add(keyArray[index], valueArray[index]);
            }

            return dict;
        }


        /// <summary>
        /// Creates a valid XML tag pair in the XmlWriter, based upon the Data object passed in.
        /// This is used internally for a DataSet, DataTable or DataRow
        /// </summary>
        /// <param name="writer">The XmlWriter to add the XML Tag.</param>
        /// <param name="value">An object to represent in the PropertyList.</param>
        private static void createDocumentFragmentFromDataObject(XmlWriter writer, object value)
        {
            Type objectType = value.GetType();

            // Creates an array of DataTables.
            if (objectType.Equals(typeof(System.Data.DataSet)))
            {
                DataSet dataset = value as DataSet;

                writer.WriteStartElement("array");

                foreach (DataTable table in dataset.Tables)
                {
                    createDocumentFragmentFromDataObject(writer, table);
                }

                writer.WriteEndElement();       // 'array' tag
            }
            else if (objectType.Equals(typeof(System.Data.DataTable)))
            {
                // Creates an array of DataRows.
                DataTable table = value as DataTable;

                writer.WriteStartElement("array");

                foreach (DataRow row in table.Rows)
                {
                    createDocumentFragmentFromDataObject(writer, row);
                }

                writer.WriteEndElement();       // 'array' tag
            }
            else if (objectType.Equals(typeof(System.Data.DataRow)))
            {
                // With the DataRows we need to cater for potential DBNull values.
                // This needs to be handled on a per-type basis, as most cannot be null.
                DataRow row = value as DataRow;

                writer.WriteStartElement("dict");

                foreach (DataColumn column in row.Table.Columns)
                {
                    string objType = column.DataType.ToString();
                    bool isNull = row[column].Equals(DBNull.Value);

                    writer.WriteElementString("key", column.ColumnName);

                    if (objType.Equals("System.String"))
                    {
                        createDocumentFragment(writer, row[column].ToString(), false);
                    }
                    else if (objType.StartsWith("System.Int16") ||
                        objType.StartsWith("System.Int32") ||
                        objType.StartsWith("System.Int64"))
                    {
                        if (isNull)
                        {
                            createDocumentFragment(writer, 0, false);
                        }
                        else
                        {
                            Int64 intData = Convert.ToInt64(row[column]);
                            createDocumentFragment(writer, intData, false);
                        }
                    }
                    else if (objType.Equals("System.Boolean"))
                    {
                        if (isNull)
                        {
                            createDocumentFragment(writer, false, false);
                        }
                        else
                        {
                            bool boolData = Convert.ToBoolean(row[column]);
                            createDocumentFragment(writer, boolData, false);
                        }
                    }
                    else if (objType.Equals("System.Single") ||
                       objType.Equals("System.Double") ||
                       objType.Equals("System.Decimal"))
                    {
                        if (isNull)
                        {
                            createDocumentFragment(writer, (decimal)0, false);
                        }
                        else
                        {
                            // Decimal will retain the precision of a Single (float)
                            // or Double, but not vice-versa.
                            Decimal decimalData = Convert.ToDecimal(row[column]);
                            createDocumentFragment(writer, decimalData, false);
                        }
                    }
                    else if (objType.Equals("System.DateTime"))
                    {
                        if (isNull)
                        {
                            createDocumentFragment(writer, DateTime.MinValue, false);
                        }
                        else
                        {
                            DateTime dateData = Convert.ToDateTime(row[column]);
                            createDocumentFragment(writer, dateData, false);
                        }
                    }
                }

                writer.WriteEndElement();        // 'dict' Tag
            }
        }
    }
}