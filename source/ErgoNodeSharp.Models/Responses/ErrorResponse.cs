using System.Collections.Generic;

namespace ErgoNodeSharp.Models.Responses
{
    public class ErrorResponse
    {
        public IList<ErrorMessage> Errors { get; set; }

        public ErrorResponse()
        {
            Errors = new List<ErrorMessage>();
        }

        public ErrorResponse(ErrorMessage errorMessage) : this()
        {
            Errors.Add(errorMessage);
        }
    }

    public class ErrorMessage
    {
        public string Status { get; set; }
        public string Title { get; set; }

        public string Detail { get; set; }
    }
}
