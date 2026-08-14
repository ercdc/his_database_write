using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using HisMedicalRecordFillDemo.Exceptions;

namespace HisMedicalRecordFillDemo.Services;

public interface IXmlRecordValidator
{
    void Validate(string xml, string schemaPath);
}

public sealed class XmlRecordValidator : IXmlRecordValidator
{
    public void Validate(string xml, string schemaPath)
    {
        if (string.IsNullOrWhiteSpace(xml))
            throw new XmlValidationException("工具参数 xml 不能为空。 ");

        var errors = new List<string>();
        var schemas = new XmlSchemaSet();
        schemas.Add(null, schemaPath);
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            ValidationType = ValidationType.Schema,
            Schemas = schemas
        };
        settings.ValidationEventHandler += (_, eventArgs) => errors.Add(eventArgs.Message);

        XDocument document;
        try
        {
            using var reader = XmlReader.Create(new StringReader(xml), settings);
            document = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
        }
        catch (XmlException exception)
        {
            throw new XmlValidationException($"XML 无法解析或不符合 XSD：{exception.Message}");
        }

        if (document.Root?.Name.LocalName != "首次病程记录")
            throw new XmlValidationException("XML 根节点必须是“首次病程记录”。 ");
        if (errors.Count > 0)
            throw new XmlValidationException($"XML 不符合 XSD：{string.Join("；", errors)}");
    }
}
