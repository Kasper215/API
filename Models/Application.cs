namespace WebApplication1.Models
{
    /// <summary>
    /// Класс заявки на поступление
    /// </summary>
    public class Application
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; } // Id абитуриента
        public string? Specialty1 { get; set; } // Выбранная специальность №1
        public string? Specialty2 { get; set; } // Выбранная специальность №2
        public string? Result { get; set; } // Результат обработки
        public string? NotificationText { get; set; } // Текст уведомления
    }

    /// <summary>
    /// Модель для получения данных при подаче заявки (POST /api/applications)
    /// </summary>
    public class CreateApplicationRequest
    {
        public int ApplicantId { get; set; }
        public string? Specialty1 { get; set; }
        public string? Specialty2 { get; set; }
    }

    /// <summary>
    /// Модель для подтверждения поступления (POST /api/applications/{id}/confirm)
    /// </summary>
    public class ConfirmApplicationRequest
    {
        public bool Confirm { get; set; }
    }
}
