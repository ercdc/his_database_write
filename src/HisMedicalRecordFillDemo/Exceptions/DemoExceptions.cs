namespace HisMedicalRecordFillDemo.Exceptions;

public sealed class FixtureNotFoundException : Exception
{
    public FixtureNotFoundException(string message) : base(message) { }
}

public sealed class ToolCallingException : Exception
{
    public ToolCallingException(string message) : base(message) { }
}

public sealed class XmlValidationException : Exception
{
    public XmlValidationException(string message) : base(message) { }
}
