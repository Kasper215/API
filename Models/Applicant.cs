namespace WebApplication1.Models
{
    /// <summary>
    /// Класс абитуриента
    /// </summary>
    public class Applicant
    {
        public int Id { get; set; }
        public string? LastName { get; set; } // Фамилия
        public string? FirstName { get; set; } // Имя
        public string? Patronymic { get; set; } // Отчество
        
        public double CertificateScore { get; set; } // Балл аттестата
        public double RussianScore { get; set; } // Балл ЕГЭ по русскому языку
        public double SubjectScore { get; set; } // Балл ЕГЭ по профильному предмету

        // Итоговый балл — рассчитывается автоматически по формуле
        public double TotalScore => (CertificateScore * 0.4) + (RussianScore * 0.3) + (SubjectScore * 0.3);

        public string? Status { get; set; } = "абитуриент"; // "абитуриент" или "студент"
    }
}
