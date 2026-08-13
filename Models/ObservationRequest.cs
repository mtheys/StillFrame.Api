namespace StillFrame.Api.Models
{
    public class ObservationRequest
    {
        public int StudyId { get; set; }
        public int ObservationStationId { get; set; }
        public int ObjectCount { get; set; }
        public DateTime CapturedAt { get; set; }
        public decimal? AverageConfidence { get; set; }
        public int? ProcessingMilliseconds { get; set; }
        public string? DeviceIdentifier { get; set; }
    }
}