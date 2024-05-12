namespace HelpDesk.Models
{
    public class Ticket
    {

        public int id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime CloseDate { get; set; }
        public int? StatusId { get; set; }
        public int? UserId { get; set; }
        public int? SupporterId { get; set; }
        public int? AdminId { get; set; }

    }
}
