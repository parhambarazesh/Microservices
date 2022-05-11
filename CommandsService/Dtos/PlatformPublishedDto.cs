namespace CommandsService.Dtos
{
    public class PlatformPublishedDto
    {
        // This is going to map to ExternalID in the Platform model.
        public int Id { get; set; }
        public string Name { get; set; }
        public string Event { get; set; }
    }
}