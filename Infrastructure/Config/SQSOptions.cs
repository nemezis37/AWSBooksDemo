namespace Infrastructure.Config
{
    public class SQSOptions
    {
        public const string SQSOptionsSectionName = "SQS";

        public string QueueUrl { get; set; }
    }
}
