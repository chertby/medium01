using System.Runtime.Serialization;

namespace ProblemDetailsCustomization.WebApi;

[Serializable]
public sealed class ApplicationSpecificException : Exception
{
    public ApplicationSpecificException()
    {
    }

    public ApplicationSpecificException(string message) : base(message)
    {
    }

    private ApplicationSpecificException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}