using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class AdmissionService
    {
        // База данных в памяти (статические списки)
        private static readonly List<Specialty> Specialties = new List<Specialty>
        {
            new Specialty { Id = 1, Name = "Информатика и вычислительная техника", PassingScore = 80 },
            new Specialty { Id = 2, Name = "Информационная безопасность", PassingScore = 85 },
            new Specialty { Id = 3, Name = "Радиофизика", PassingScore = 70 }
        };

        // Несколько предзаполненных абитуриентов для удобного тестирования
        private static readonly List<Applicant> Applicants = new List<Applicant>
        {
            new Applicant { Id = 1, LastName = "Иванов", FirstName = "Иван", Patronymic = "Иванович", CertificateScore = 100, RussianScore = 90, SubjectScore = 85 },
            new Applicant { Id = 2, LastName = "Смирнов", FirstName = "Алексей", Patronymic = "Сергеевич", CertificateScore = 60, RussianScore = 50, SubjectScore = 50 }
        };

        private static readonly List<Application> Applications = new List<Application>();
        private static int _applicationIdCounter = 1;

        public Task<List<Specialty>> GetSpecialtiesAsync() => Task.FromResult(Specialties);

        public Task<List<Applicant>> GetApplicantsAsync() => Task.FromResult(Applicants);

        /// <summary>
        /// Обработка заявки на поступление
        /// </summary>
        public Task<Application?> ProcessApplicationAsync(CreateApplicationRequest request)
        {
            var applicant = Applicants.FirstOrDefault(a => a.Id == request.ApplicantId);
            if (applicant == null) return Task.FromResult<Application?>(null);

            var spec1 = Specialties.FirstOrDefault(s => s.Name == request.Specialty1);
            var spec2 = Specialties.FirstOrDefault(s => s.Name == request.Specialty2);

            // Проверка прохождения по баллам
            bool passedSpec1 = spec1 != null && applicant.TotalScore >= spec1.PassingScore;
            bool passedSpec2 = spec2 != null && applicant.TotalScore >= spec2.PassingScore;

            var application = new Application
            {
                Id = _applicationIdCounter++,
                ApplicantId = request.ApplicantId,
                Specialty1 = request.Specialty1,
                Specialty2 = request.Specialty2
            };

            // Логика сценариев
            if (!passedSpec1 && !passedSpec2)
            {
                // Сценарий А
                application.Result = "Не набрал проходной балл";
                application.NotificationText = "К сожалению, вы не набрали проходной балл ни на одну из выбранных специальностей.";
            }
            else if (passedSpec1 && passedSpec2)
            {
                // Сценарий В
                application.Result = "Прошёл на обе";
                application.NotificationText = "Поздравляем! Вы прошли на обе специальности. Пожалуйста, выберите одну из двух для зачисления.";
            }
            else
            {
                // Сценарий Б
                string passedSpecialty = passedSpec1 ? request.Specialty1 : request.Specialty2;
                application.Result = $"Прошёл на {passedSpecialty}";
                application.NotificationText = $"Поздравляем! Вы прошли на специальность '{passedSpecialty}'. Желаете подтвердить поступление?";
            }

            Applications.Add(application);
            return Task.FromResult(application);
        }

        /// <summary>
        /// Подтверждение или отказ от поступления
        /// </summary>
        public Task<bool> ConfirmAdmissionAsync(int applicationId, bool isConfirmed)
        {
            var application = Applications.FirstOrDefault(a => a.Id == applicationId);
            if (application == null) return Task.FromResult(false);

            var applicant = Applicants.FirstOrDefault(a => a.Id == application.ApplicantId);
            if (applicant == null) return Task.FromResult(false);

            if (isConfirmed)
            {
                applicant.Status = "студент";
                application.NotificationText = "Вы зачислены!";
            }
            else
            {
                // Статус остаётся "абитуриент"
                application.NotificationText = "Вы не зачислены";
            }

            return Task.FromResult(true);
        }
    }
}
