namespace RESR.Models.Marks
{
    public class Mark
    {
        public int IdMark { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsReadLater { get; set; }
        public int IdRessource { get; set; } 
        public int IdUser { get; set; } 
    }
}