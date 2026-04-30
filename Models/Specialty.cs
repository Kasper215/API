namespace WebApplication1.Models
{
    /// <summary>
    /// Класс специальности
    /// </summary>
    public class Specialty
    {
        public int Id { get; set; }
        public string? Name { get; set; } // Наименование
        public int PassingScore { get; set; } // Проходной балл
    }
}
