﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pusula.Training.HealthCare.Appointments;
using Pusula.Training.HealthCare.AppointmentTypes;
using Pusula.Training.HealthCare.Cities;
using Pusula.Training.HealthCare.BloodTests.Categories;
using Pusula.Training.HealthCare.BloodTests.Tests;
using Pusula.Training.HealthCare.Departments;
using Pusula.Training.HealthCare.Districts;
using Pusula.Training.HealthCare.DoctorLeaves;
using Pusula.Training.HealthCare.Doctors;
using Pusula.Training.HealthCare.DoctorWorkingHours;
using Pusula.Training.HealthCare.MedicalPersonnel;
using Pusula.Training.HealthCare.Insurances;
using Pusula.Training.HealthCare.MedicalServices;
using Pusula.Training.HealthCare.Patients;
using Pusula.Training.HealthCare.Permissions;
using Pusula.Training.HealthCare.Protocols;
using Pusula.Training.HealthCare.ProtocolTypes;
using Pusula.Training.HealthCare.Restrictions;
using Pusula.Training.HealthCare.Titles;
using Pusula.Training.HealthCare.Treatment.Examinations;
using Pusula.Training.HealthCare.Treatment.Examinations.Backgrounds;
using Pusula.Training.HealthCare.Treatment.Examinations.FamilyHistories;
using Pusula.Training.HealthCare.Treatment.Examinations.PhysicalFindings;
using Pusula.Training.HealthCare.Treatment.Icds;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;

namespace Pusula.Training.HealthCare
{
    public class HealthCareDataSeederContributor(
        IdentityRoleManager roleManager,
        IPermissionManager permissionManager,
        IdentityUserManager userManager,
        IPatientRepository patientRepository,
        IDepartmentRepository departmentRepository,
        IAppointmentRepository appointmentRepository,
        IAppointmentTypeRepository appointmentTypeRepository,
        ITitleRepository titleRepository,
        IMedicalServiceRepository medicalServiceRepository,
        IMedicalServiceManager medicalServiceManager,
        IRepository<Doctor> doctorRepository,
        IRepository<DoctorWorkingHour> doctorWorkingHourRepository,
        ICityRepository cityRepository,
        IDistrictRepository districtRepository,
        IGuidGenerator guidGenerator,
        ITestCategoryRepository testCategoryRepository,
        IProtocolTypeRepository protocolTypeRepository,
        IProtocolRepository protocolRepository,
        IProtocolManager protocolManager,
        IIcdRepository icdRepository,
        IExaminationRepository examinationRepository,
        IFamilyHistoryRepository familyHistoryRepository,
        IBackgroundRepository backgroundRepository,
        IPhysicalFindingRepository physicalFindingRepository,
        IMedicalStaffRepository medicalStaffRepository,
        IInsuranceRepository insuranceRepository,
        IRestrictionManager restrictionManager,
        IDoctorLeaveRepository leaveRepository,
        ITestRepository testRepository) : IDataSeedContributor, ITransientDependency
    {
        public async Task SeedAsync(DataSeedContext context)
        {
            await SeedCityRecords();
            await SeedAppointmentTypes();
            await SeedPatientRecords();
            await SeedRoleRecords();
            await SeedDistrictRecords();
            await SeedMedicalServiceRecords();
            await SeedDepartmentRecords();
            await SeedMedicalServiceToDepartments();
            await SeedTitles();
            await SeedDoctorRecords();
            await SeedMedicalStaffRecords();
            await SeedDoctorWorkingHours();
            await SeedDoctorLeaves();
            await SeedAppointments();
            await SeedProtocolType();
            await SeedProtocols();
            await SeedIcds();
            await SeedExaminations();
            await SeedFamilyHistory();
            await SeedBackground();
            await SeedPhysicalFindings();
            await SeedTestCategoryRecords();
            await SeedTestRecords();
            await SeedMedicalServiceRestrictions();
           await SeedProtocolMedicalServices();
        }

        private async Task SeedProtocolType()
        {
            var types = new List<ProtocolType>
            {
                new ProtocolType(guidGenerator.Create(), "Ayakta"),
                new ProtocolType(guidGenerator.Create(), "Yatış"),
                new ProtocolType(guidGenerator.Create(), "Kontrol"),
                new ProtocolType(guidGenerator.Create(), "Acil"),
                new ProtocolType(guidGenerator.Create(), "Poliklinik"),
                new ProtocolType(guidGenerator.Create(), "Ameliyat"),
                new ProtocolType(guidGenerator.Create(), "Fizik Tedavi"),
                new ProtocolType(guidGenerator.Create(), "Diyetisyen"),
                new ProtocolType(guidGenerator.Create(), "Radyoloji"),
                new ProtocolType(guidGenerator.Create(), "Laboratuvar"),
                new ProtocolType(guidGenerator.Create(), "Psikiyatri"),
                new ProtocolType(guidGenerator.Create(), "Göz Muayenesi")
            };

            await protocolTypeRepository.InsertManyAsync(types, autoSave: true);
        }

        private async Task SeedTitles()
        {
            var titles = new List<Title>
            {
                new Title(guidGenerator.Create(), "Dr."),
                new Title(guidGenerator.Create(), "Prof."),
                new Title(guidGenerator.Create(), "Yrd. Doç."),
                new Title(guidGenerator.Create(), "Op."),
            };

            await titleRepository.InsertManyAsync(titles, autoSave: true);
        }

        private async Task SeedMedicalStaffRecords()
        {
            if (await departmentRepository.GetCountAsync() == 0
                || await cityRepository.GetCountAsync() == 0
                || await districtRepository.GetCountAsync() == 0)
            {
                return;
            }

            var departments = await departmentRepository.GetListAsync();
            var cityTitles = await cityRepository.GetListAsync();
            var districtTitles = await districtRepository.GetListWithNavigationPropertiesAsync();

            var city = cityTitles.First(x => x.Name == "Ankara");
            var district = districtTitles.First(d => d.City.Name == "Ankara");

            var d1 = new MedicalStaff(
                guidGenerator.Create(),
                city.Id,
                district.District.Id,
                departments[0].Id,
                "Arif",
                "Yılmaz",
                "12345678901",
                new DateTime(1980, 5, 12),
                EnumGender.MALE,
                new DateTime(1999, 5, 12),
                "ahmet.yilmaz@example.com",
                "555-1234567"
            );

            var d2 = new MedicalStaff(
                guidGenerator.Create(),
                city.Id,
                district.District.Id,
                departments[0].Id,
                "Fatma",
                "Kara",
                "98765432109",
                new DateTime(1990, 3, 25),
                EnumGender.FEMALE,
                new DateTime(2001, 5, 12),
                "fatma.kara@example.com",
                "555-9876543"
            );

            var d3 = new MedicalStaff(
                guidGenerator.Create(),
                city.Id,
                district.District.Id,
                departments[0].Id,
                "Mehmet",
                "Çelik",
                "12309876543",
                new DateTime(1975, 11, 30),
                EnumGender.MALE,
                new DateTime(2005, 11, 30),
                "mehmet.celik@example.com",
                "555-3219876"
            );

            var d4 = new MedicalStaff(
                guidGenerator.Create(),
                city.Id,
                district.District.Id,
                departments[1].Id,
                "Merve",
                "Şahin",
                "23456789012",
                new DateTime(1985, 8, 23),
                EnumGender.FEMALE,
                new DateTime(2005, 11, 30),
                "merve.sahin@example.com",
                "555-2345678"
            );

            var d5 = new MedicalStaff(
                guidGenerator.Create(),
                city.Id,
                district.District.Id,
                departments[1].Id,
                "Zeynep",
                "Demir",
                "45678901234",
                new DateTime(1990, 7, 5),
                EnumGender.FEMALE,
                new DateTime(2009, 7, 5),
                "zeynep.demir@example.com",
                "555-4567890"
            );

            var d6 = new MedicalStaff(
                guidGenerator.Create(),
                city.Id,
                district.District.Id,
                departments[3].Id,
                "Ahmet",
                "Aksoy",
                "56789012345",
                new DateTime(1982, 11, 30),
                EnumGender.MALE,
                new DateTime(2012, 7, 5),
                "ahmet.aksoy@example.com",
                "555-5678901"
            );

            var d7 = new MedicalStaff(
                guidGenerator.Create(),
                city.Id,
                district.District.Id,
                departments[2].Id,
                "Elif",
                "Çelik",
                "67890123456",
                new DateTime(1988, 4, 18),
                EnumGender.FEMALE,
                new DateTime(2017, 4, 18),
                "elif.celik@example.com",
                "555-6789012"
            );

            var d8 = new MedicalStaff(
                guidGenerator.Create(),
                city.Id,
                district.District.Id,
                departments[4].Id,
                "Mehmet",
                "Güneş",
                "78901234567",
                new DateTime(1970, 9, 9),
                EnumGender.MALE,
                new DateTime(2019, 9, 9),
                "mehmet.gunes@example.com",
                "555-7890123"
            );

            var d9 = new MedicalStaff(
                guidGenerator.Create(),
                city.Id,
                district.District.Id,
                departments[5].Id,
                "Ayşe",
                "Yıldız",
                "89012345678",
                new DateTime(1987, 6, 22),
                EnumGender.FEMALE,
                new DateTime(2023, 6, 22),
                "ayse.yildiz@example.com",
                "555-8901234"
            );

            await medicalStaffRepository.InsertAsync(d1, true);
            await medicalStaffRepository.InsertAsync(d2, true);
            await medicalStaffRepository.InsertAsync(d3, true);
            await medicalStaffRepository.InsertAsync(d4, true);
            await medicalStaffRepository.InsertAsync(d5, true);
            await medicalStaffRepository.InsertAsync(d6, true);
            await medicalStaffRepository.InsertAsync(d7, true);
            await medicalStaffRepository.InsertAsync(d8, true);
            await medicalStaffRepository.InsertAsync(d9, true);
        }

        private async Task SeedMedicalServiceRecords()
        {
            if (await medicalServiceRepository.GetCountAsync() > 0)
            {
                return;
            }

            await medicalServiceRepository.InsertAsync(
                new MedicalService(Guid.NewGuid(), "X-Ray", 300.00, 30, DateTime.Now), true);
            await medicalServiceRepository.InsertAsync(new MedicalService(Guid.NewGuid(), "MRI Scan", 1200.00, 30,
                DateTime.Now), true);
            await medicalServiceRepository.InsertAsync(new MedicalService(Guid.NewGuid(), "Cardiology Consultation",
                2500.00, 30, DateTime.Now), true);

            await medicalServiceRepository.InsertAsync(new MedicalService(Guid.NewGuid(), "Pediatric Check-up",
                1200.00, 30, DateTime.Now), true);
            await medicalServiceRepository.InsertAsync(
                new MedicalService(Guid.NewGuid(), "Chiropractic Session", 9000.00, 30, DateTime.Now));
            await medicalServiceRepository.InsertAsync(new MedicalService(Guid.NewGuid(), "Nutrition Consultation",
                750.00, 30, DateTime.Now), true);

            await medicalServiceRepository.InsertAsync(new MedicalService(Guid.NewGuid(), "Psychiatric Evaluation",
                3000.0, 30, DateTime.Now), true);
            await medicalServiceRepository.InsertAsync(new MedicalService(Guid.NewGuid(), "Radiology Review", 9100.00,
                30, DateTime.Now), true);
            await medicalServiceRepository.InsertAsync(new MedicalService(Guid.NewGuid(), "Physical Therapy Assessment",
                725.00, 30, DateTime.Now), true);

            await medicalServiceRepository.InsertAsync(new MedicalService(Guid.NewGuid(), "Obstetrics Ultrasound",
                4000.00, 30, DateTime.Now), true);
            await medicalServiceRepository.InsertAsync(new MedicalService(Guid.NewGuid(), "Emergency Room Visit",
                5000.00, 30, DateTime.Now), true);

            await medicalServiceRepository.InsertAsync(new MedicalService(Guid.NewGuid(), "Examination",
                1000.00, 20, DateTime.Now), true);
        }

        private async Task SeedDepartmentRecords()
        {
            if (await departmentRepository.GetCountAsync() > 0)
            {
                return;
            }

            await departmentRepository.InsertAsync(new Department(Guid.NewGuid(), "Cardiology"), true);
            await departmentRepository.InsertAsync(new Department(Guid.NewGuid(), "Radiology"), true);
            await departmentRepository.InsertAsync(new Department(Guid.NewGuid(), "Emergency"), true);
            await departmentRepository.InsertAsync(new Department(Guid.NewGuid(), "Pediatrics"), true);
            await departmentRepository.InsertAsync(new Department(Guid.NewGuid(), "Orthopedics"), true);
            await departmentRepository.InsertAsync(new Department(Guid.NewGuid(), "Dermatology"), true);
            await departmentRepository.InsertAsync(new Department(Guid.NewGuid(), "Urology"), true);
            await departmentRepository.InsertAsync(new Department(Guid.NewGuid(), "Oncology"), true);
            await departmentRepository.InsertAsync(new Department(Guid.NewGuid(), "Neurology"), true);
        }

        private async Task SeedMedicalServiceRestrictions()
        {
            if (await medicalServiceRepository.GetCountAsync() == 0
                || await departmentRepository.GetCountAsync() == 0
                || await departmentRepository.GetCountAsync() == 0)
            {
                return;
            }

            var medicalServices = await medicalServiceRepository.GetListAsync(includeDetails: true);
            var departments = await departmentRepository.GetListAsync(includeDetails: true);
            var doctors = await doctorRepository.GetListAsync(includeDetails: true);
            var medicalServiceIndex = 0;

            foreach (var doctor in doctors)
            {
                if (medicalServiceIndex + 1 > medicalServices.Count)
                {
                    return;
                }

                var medicalService = medicalServices[medicalServiceIndex++];

                await restrictionManager.CreateAsync(
                    medicalService.Id,
                    doctor.Department.Id,
                    doctor.Id,
                    new Random().Next(18, 50),
                    new Random().Next(51, 80),
                    (EnumGender)new Random().Next(0, 2));
            }
        }

        private async Task SeedMedicalServiceToDepartments()
        {
            if (await medicalServiceRepository.GetCountAsync() == 0
                || await departmentRepository.GetCountAsync() == 0)
            {
                return;
            }

            var medicalServices = await medicalServiceRepository.GetListAsync(includeDetails: true);
            var departments = await departmentRepository.GetListAsync(includeDetails: true);

            var cardiology = departments.First(d => d.Name == "Cardiology");
            var radiology = departments.First(d => d.Name == "Radiology");
            var emergency = departments.First(d => d.Name == "Emergency");
            var orthopedics = departments.First(d => d.Name == "Orthopedics");
            var pediatrics = departments.First(d => d.Name == "Pediatrics");

            var cardiologyConsult = medicalServices.First(ms => ms.Name == "Cardiology Consultation");
            var xray = medicalServices.First(ms => ms.Name == "X-Ray");
            var roomVisit = medicalServices.First(ms => ms.Name == "Emergency Room Visit");
            var physical = medicalServices.First(ms => ms.Name == "Physical Therapy Assessment");
            var ultrasound = medicalServices.First(ms => ms.Name == "Obstetrics Ultrasound");
            var radiologyReview = medicalServices.First(ms => ms.Name == "Radiology Review");
            var nutrition = medicalServices.First(ms => ms.Name == "Nutrition Consultation");
            var examination = medicalServices.First(ms => ms.Name == "Examination");

            //cardiologyConsultation
            await medicalServiceManager.UpdateAsync(cardiologyConsult.Id, cardiologyConsult.Name,
                cardiologyConsult.Cost,
                cardiologyConsult.Duration, cardiologyConsult.ServiceCreatedAt, [cardiology.Name]);

            //Xray
            await medicalServiceManager.UpdateAsync(xray.Id, xray.Name, xray.Cost,
                xray.Duration, xray.ServiceCreatedAt, [radiology.Name, emergency.Name]);

            //roomVisit
            await medicalServiceManager.UpdateAsync(roomVisit.Id, roomVisit.Name, roomVisit.Cost,
                roomVisit.Duration, roomVisit.ServiceCreatedAt, [emergency.Name]);

            //physicalTherapy
            await medicalServiceManager.UpdateAsync(physical.Id, physical.Name, physical.Cost,
                physical.Duration, physical.ServiceCreatedAt, [orthopedics.Name, emergency.Name]);

            //ultrasound
            await medicalServiceManager.UpdateAsync(ultrasound.Id, ultrasound.Name, ultrasound.Cost,
                ultrasound.Duration, ultrasound.ServiceCreatedAt, [radiology.Name, emergency.Name, cardiology.Name]);

            //radiologyReview
            await medicalServiceManager.UpdateAsync(radiologyReview.Id, radiologyReview.Name, radiologyReview.Cost,
                radiologyReview.Duration, radiologyReview.ServiceCreatedAt,
                [radiology.Name, emergency.Name, cardiology.Name]);

            //nutrition
            await medicalServiceManager.UpdateAsync(nutrition.Id, nutrition.Name, nutrition.Cost,
                nutrition.Duration, nutrition.ServiceCreatedAt, [emergency.Name, cardiology.Name]);

            //examination
            await medicalServiceManager.UpdateAsync(examination.Id, examination.Name, examination.Cost,
                examination.Duration, examination.ServiceCreatedAt,
                [emergency.Name, cardiology.Name, pediatrics.Name, orthopedics.Name, radiology.Name]);
        }

        private async Task SeedProtocolMedicalServices()
        {
            // Eğer protokol ya da tıbbi hizmet tablosunda veri yoksa işlemi sonlandır
            if (await protocolRepository.GetCountAsync() == 0 || await medicalServiceRepository.GetCountAsync() == 0)
            {
                return;
            }

            // Protokoller ve tıbbi hizmetler alınıyor
            var protocols = await protocolRepository.GetListAsync();
            var medicalServices = await medicalServiceRepository.GetListAsync();

            // Eğer protokol ya da tıbbi hizmet bulunamazsa işlemi sonlandır
            if (!protocols.Any() || !medicalServices.Any())
            {
                throw new Exception("No protocols or medical services found. Ensure the data is seeded correctly.");
            }

            var random = new Random();

            foreach (var protocol in protocols)
            {
                // Eğer ProtocolMedicalServices koleksiyonu null ise, yeni bir liste oluştur

                // Rastgele 5 tıbbi hizmet seç
                var selectedServices =
                    (medicalServices.OrderBy(_ => random.Next()).Take(5).ToList()).Select(x => x.Name);


                await protocolManager.UpdateAsync(protocol.Id, selectedServices.ToArray(), protocol.PatientId,
                    protocol.DepartmentId, protocol.ProtocolTypeId, protocol.DoctorId, protocol.InsuranceId,
                    protocol.StartTime, protocol.Note, protocol.EndTime, null);
                // Eğer seçilen hizmet daha önce eklenmemişse ekle
            }
        }

        private async Task SeedPatientRecords()
        {
            if (await patientRepository.GetCountAsync() > 0)
            {
                return;
            }

            var patient1 = new Patient(
                Guid.NewGuid(),
                1,
                "Hasan",
                "Kuru",
                EnumGender.MALE,
                new DateTime(1990, 1, 1),
                "76863495742",
                "SS3278363",
                "Turkey",
                "+12345678901",
                EnumPatientTypes.VIP,
                "Fatma",
                "Aykut",
                "hasan.kuru@example.com",
                EnumRelative.FATHER,
                "+12344678901",
                "İstanbul",
                EnumDiscountGroup.STAFF
            );

            var patient2 = new Patient(
                Guid.NewGuid(),
                2,
                "Liam",
                "O'Connor",
                EnumGender.MALE,
                new DateTime(1985, 5, 10),
                "5712930A",
                "61348019X",
                "Ireland",
                "+98765432109",
                EnumPatientTypes.NORMAL,
                "Sofia",
                "Anthony",
                "liam.oconnor@example.com",
                EnumRelative.MOTHER,
                "+09876543210",
                string.Empty,
                EnumDiscountGroup.STAFF
            );

            var patient3 = new Patient(
                Guid.NewGuid(),
                3,
                "Lukas",
                "Müller",
                EnumGender.FEMALE,
                new DateTime(1992, 3, 15),
                "5841728374",
                "C2692630",
                "Germany",
                "+12312312345",
                EnumPatientTypes.VIP,
                "Emma",
                "Max",
                "lukas.muller@example.com",
                EnumRelative.FATHER,
                "+11223344556",
                string.Empty,
                EnumDiscountGroup.CONTRACTED
            );

            var patient4 = new Patient(
                Guid.NewGuid(),
                4,
                "Emily",
                "Brown",
                EnumGender.FEMALE,
                new DateTime(1995, 7, 20),
                "3920485723",
                "QW8394820",
                "USA",
                "+14567891234",
                EnumPatientTypes.NORMAL,
                "Oliver",
                "Grace",
                "emily.brown@example.com",
                EnumRelative.MOTHER,
                "+15467891235",
                "New York",
                EnumDiscountGroup.NONE
            );

            var patient5 = new Patient(
                Guid.NewGuid(),
                5,
                "Isabella",
                "Garcia",
                EnumGender.FEMALE,
                new DateTime(1998, 9, 30),
                "8291749283",
                "WZ9384930",
                "Spain",
                "+34912345678",
                EnumPatientTypes.VIP,
                "Carlos",
                "Maria",
                "isabella.garcia@example.com",
                EnumRelative.MOTHER,
                "+34987654321",
                "Madrid",
                EnumDiscountGroup.CONTRACTED
            );

            var patient6 = new Patient(
                Guid.NewGuid(),
                6,
                "Ethan",
                "Wang",
                EnumGender.MALE,
                new DateTime(1987, 4, 12),
                "4729381029",
                "XY7283940",
                "China",
                "+861234567890",
                EnumPatientTypes.NORMAL,
                "Mei",
                "Li",
                "ethan.wang@example.com",
                EnumRelative.FATHER,
                "+861987654321",
                "Beijing",
                EnumDiscountGroup.STAFF
            );

            await patientRepository.InsertAsync(patient1, true);
            await patientRepository.InsertAsync(patient2, true);
            await patientRepository.InsertAsync(patient3, true);
            await patientRepository.InsertAsync(patient4, true);
            await patientRepository.InsertAsync(patient5, true);
            await patientRepository.InsertAsync(patient6, true);
        }


        private async Task SeedTestCategoryRecords()
        {
            if (await testCategoryRepository.GetCountAsync() > 0)
            {
                return;
            }

            await testCategoryRepository.InsertAsync(
                new TestCategory(Guid.NewGuid(), "Hematological Tests",
                    "Measures blood cells (red blood cells, white blood cells) and related values.", "1.png", 1500),
                true);

            await testCategoryRepository.InsertAsync(
                new TestCategory(Guid.NewGuid(), "Biochemical Tests",
                    "Measures vitamins, mineral levels and other chemical values.", "2.jpg", 2000), true);

            await testCategoryRepository.InsertAsync(new TestCategory(Guid.NewGuid(), "Hormonal Tests",
                "Measures hormone levels and endocrine functions.",
                "3.jpg", 2500), true);
        }

        private async Task SeedTestRecords()
        {
            var testCount = await testRepository.CountAsync();
            if (testCount > 0)
            {
                return;
            }

            var hematologicalCategory =
                await testCategoryRepository.FirstOrDefaultAsync(tc => tc.Name == "Hematological Tests");
            var biochemicalCategory =
                await testCategoryRepository.FirstOrDefaultAsync(tc => tc.Name == "Biochemical Tests");
            var hormonalCategory = await testCategoryRepository.FirstOrDefaultAsync(tc => tc.Name == "Hormonal Tests");

            if (hematologicalCategory == null || biochemicalCategory == null || hormonalCategory == null)
            {
                throw new Exception("Test categories not found.");
            }

            var testList = new List<Test>
            {
                new Test(guidGenerator.Create(), hematologicalCategory.Id, "Hemoglobin", 13.5, 17.5),
                new Test(guidGenerator.Create(), hematologicalCategory.Id, "Hematocrit", 38.0, 50.0),
                new Test(guidGenerator.Create(), hematologicalCategory.Id, "White Blood Cell Count", 4.0, 11.0),

                new Test(guidGenerator.Create(), biochemicalCategory.Id, "Glucose", 70, 99),
                new Test(guidGenerator.Create(), biochemicalCategory.Id, "Cholesterol", 125, 200),
                new Test(guidGenerator.Create(), biochemicalCategory.Id, "Creatinine", 0.6, 1.2),

                new Test(guidGenerator.Create(), hormonalCategory.Id, "TSH", 0.5, 5.5),
                new Test(guidGenerator.Create(), hormonalCategory.Id, "Free T3", 2.3, 4.2),
                new Test(guidGenerator.Create(), hormonalCategory.Id, "Free T4", 0.7, 1.8)
            };
            await testRepository.InsertManyAsync(testList, true);
        }

        private async Task SeedCityRecords()
        {
            if (await cityRepository.GetCountAsync() > 0)
            {
                return;
            }

            var c1 = new City(guidGenerator.Create(), "Istanbul");
            var c2 = new City(guidGenerator.Create(), "Ankara");
            var c3 = new City(guidGenerator.Create(), "İzmir");
            var c4 = new City(guidGenerator.Create(), "Antalya");
            var c5 = new City(guidGenerator.Create(), "Bursa");

            await cityRepository.InsertManyAsync([c1, c2, c3, c4, c5], true);
        }

        private async Task SeedDistrictRecords()
        {
            if (await districtRepository.GetCountAsync() > 0)
                return;

            var cityDistricts = new Dictionary<string, List<string>>
            {
                { "Istanbul", ["Kadıköy", "Üsküdar", "Pendik", "Bakırköy", "Sarıyer"] },
                { "Ankara", ["Çankaya", "Keçiören", "Yenimahalle", "Mamak", "Altındağ"] },
                { "Izmir", ["Konak", "Bornova", "Karşıyaka", "Buca", "Gaziemir"] },
                { "Bursa", ["Nilüfer", "Osmangazi", "Yıldırım", "Gemlik", "İnegöl"] },
                { "Antalya", ["Muratpaşa", "Kepez", "Konyaaltı", "Alanya", "Manavgat"] }
            };

            var cities = await cityRepository.GetListAsync();

            foreach (var city in cities)
            {
                if (!cityDistricts.TryGetValue(city.Name, out var districts))
                {
                    continue;
                }

                foreach (var district in districts.Select(districtName =>
                             new District(guidGenerator.Create(), city.Id, districtName)))
                {
                    await districtRepository.InsertAsync(district);
                }
            }
        }

        private async Task SeedDoctorRecords()
        {
            if (await titleRepository.GetCountAsync() == 0
                || await departmentRepository.GetCountAsync() == 0
                || await cityRepository.GetCountAsync() == 0
                || await districtRepository.GetCountAsync() == 0)
            {
                return;
            }

            var titles = await titleRepository.GetListAsync();
            var departments = await departmentRepository.GetListAsync();
            var cityTitles = await cityRepository.GetListAsync();
            var districtTitles = await districtRepository.GetListWithNavigationPropertiesAsync();

            var city = cityTitles.First(x => x.Name == "Istanbul");
            var district = districtTitles.First(d => d.City.Name == "Istanbul");

            var d1 = new Doctor(
                guidGenerator.Create(),
                city.Id,
                district.District.Id,
                titles.First(t => t.TitleName == "Dr.").Id,
                departments.First(x => x.Name == "Cardiology").Id,
                "Arif",
                "Yılmaz",
                "12345678901",
                new DateTime(1980, 5, 12),
                EnumGender.MALE,
                new DateTime(1999, 5, 12),
                "ahmet.yilmaz@example.com",
                "555-1234567"
            );

            var d2 = new Doctor(
                guidGenerator.Create(),
                city.Id,
                district.District.Id,
                titles.First(t => t.TitleName == "Dr.").Id,
                departments.First(x => x.Name == "Cardiology").Id,
                "Fatma",
                "Kara",
                "98765432109",
                new DateTime(1990, 3, 25),
                EnumGender.FEMALE,
                new DateTime(2001, 5, 12),
                "fatma.kara@example.com",
                "555-9876543"
            );

            var d3 = new Doctor(
                guidGenerator.Create(),
                city.Id,
                district.District.Id,
                titles.First(t => t.TitleName == "Dr.").Id,
                departments.First(x => x.Name == "Radiology").Id,
                "Mehmet",
                "Çelik",
                "12309876543",
                new DateTime(1975, 11, 30),
                EnumGender.MALE,
                new DateTime(2005, 11, 30),
                "mehmet.celik@example.com",
                "555-3219876"
            );

            var d4 = new Doctor(
                guidGenerator.Create(),
                city.Id,
                district.District.Id,
                titles.First(t => t.TitleName == "Prof.").Id,
                departments.First(x => x.Name == "Emergency").Id,
                "Merve",
                "Şahin",
                "23456789012",
                new DateTime(1985, 8, 23),
                EnumGender.FEMALE,
                new DateTime(2005, 11, 30),
                "merve.sahin@example.com",
                "555-2345678"
            );

            var d5 = new Doctor(
                guidGenerator.Create(),
                city.Id,
                district.District.Id,
                titles.First(t => t.TitleName == "Prof.").Id,
                departments.First(x => x.Name == "Emergency").Id,
                "Zeynep",
                "Demir",
                "45678901234",
                new DateTime(1990, 7, 5),
                EnumGender.FEMALE,
                new DateTime(2009, 7, 5),
                "zeynep.demir@example.com",
                "555-4567890"
            );

            var d6 = new Doctor(
                guidGenerator.Create(),
                city.Id,
                district.District.Id,
                titles.First(t => t.TitleName == "Yrd. Doç.").Id,
                departments.First(x => x.Name == "Radiology").Id,
                "Ahmet",
                "Aksoy",
                "56789012345",
                new DateTime(1982, 11, 30),
                EnumGender.MALE,
                new DateTime(2012, 7, 5),
                "ahmet.aksoy@example.com",
                "555-5678901"
            );

            var d7 = new Doctor(
                guidGenerator.Create(),
                city.Id,
                district.District.Id,
                titles.First(t => t.TitleName == "Dr.").Id,
                departments.First(x => x.Name == "Cardiology").Id,
                "Elif",
                "Çelik",
                "67890123456",
                new DateTime(1988, 4, 18),
                EnumGender.FEMALE,
                new DateTime(2017, 4, 18),
                "elif.celik@example.com",
                "555-6789012"
            );

            var d8 = new Doctor(
                guidGenerator.Create(),
                city.Id,
                district.District.Id,
                titles.First(t => t.TitleName == "Prof.").Id,
                departments.First(x => x.Name == "Orthopedics").Id,
                "Mehmet",
                "Güneş",
                "78901234567",
                new DateTime(1970, 9, 9),
                EnumGender.MALE,
                new DateTime(2019, 9, 9),
                "mehmet.gunes@example.com",
                "555-7890123"
            );

            var d9 = new Doctor(
                guidGenerator.Create(),
                city.Id,
                district.District.Id,
                titles.First(t => t.TitleName == "Yrd. Doç.").Id,
                departments.First(x => x.Name == "Oncology").Id,
                "Ayşe",
                "Yıldız",
                "89012345678",
                new DateTime(1987, 6, 22),
                EnumGender.FEMALE,
                new DateTime(2023, 6, 22),
                "ayse.yildiz@example.com",
                "555-8901234"
            );

            await doctorRepository.InsertAsync(d1, true);
            await doctorRepository.InsertAsync(d2, true);
            await doctorRepository.InsertAsync(d3, true);
            await doctorRepository.InsertAsync(d4, true);
            await doctorRepository.InsertAsync(d5, true);
            await doctorRepository.InsertAsync(d6, true);
            await doctorRepository.InsertAsync(d7, true);
            await doctorRepository.InsertAsync(d8, true);
            await doctorRepository.InsertAsync(d9, true);
        }

        private async Task SeedProtocols()
        {
            if (await protocolRepository.GetCountAsync() > 0)
            {
                return;
            }

            var patients = await patientRepository.GetListAsync();
            var doctors = await doctorRepository.GetListAsync();
            var departments = await departmentRepository.GetListAsync();
            var protocolTypes = await protocolTypeRepository.GetListAsync();

    var random = new Random();
    var protocols = new List<Protocol>();
    var insurances = new List<Insurance>();
    int num = 0;
    foreach (var patient in patients)
    {
        for (int i = 0; i < 3; i++) // Her hasta için birden fazla protokol oluştur
        {
            var doctor = doctors.OrderBy(d => random.Next()).FirstOrDefault();
            var department = departments.FirstOrDefault(d => d.Id == doctor?.DepartmentId);
            var protocolType = protocolTypes.OrderBy(pt => random.Next()).FirstOrDefault();
            var startTime = DateTime.Now.AddDays(random.Next(1, 10));
            var endTime = startTime.AddHours(random.Next(1, 3));

                    if (doctor != null && department != null && protocolType != null)
                    {
                        // Protokole özel bir sigorta oluşturuluyor
                        var insurance = new Insurance(
                            id: Guid.NewGuid(),
                            policyNumber: $"POL-{random.Next(1000, 9999)}",
                            (EnumInsuranceCompanyName)random.Next(1, 10),
                            premiumAmount: random.Next(100, 1000),
                            coverageAmount: random.Next(1000, 10000),
                            startDate: DateTime.UtcNow.AddDays(-30),
                            endDate: DateTime.UtcNow.AddYears(1),
                            description: "Insurance for protocol"
                        );

                        insurances.Add(insurance); // Sigortayı listeye ekliyoruz

                protocols.Add(new Protocol(
                    id: Guid.NewGuid(),
                    patientId: patient.Id,
                    departmentId: department.Id,
                    doctorId: doctor.Id,
                    protocolTypeId: protocolType.Id,
                    startTime: startTime,
                    note: "Routine checkup",
                    endTime: endTime,
                    insuranceId: insurance.Id // Sigorta ID'si atanıyor
                ));
            }
        }
        if(num == 3) { break; }
        num++;
    }

            await insuranceRepository.InsertManyAsync(insurances, autoSave: true); // Sigortalar veri tabanına ekleniyor
            await protocolRepository.InsertManyAsync(protocols, autoSave: true); // Protokoller veri tabanına ekleniyor
        }

        private async Task SeedDoctorLeaves()
        {
            if (await doctorRepository.GetCountAsync() == 0)
            {
                return;
            }

            var doctors = await doctorRepository.GetListAsync();

            var leave1 = new DoctorLeave(guidGenerator.Create(), doctors[0].Id, DateTime.Today.AddDays(1),
                DateTime.Today.AddDays(7), EnumLeaveType.Normal);

            var leave2 = new DoctorLeave(guidGenerator.Create(), doctors[1].Id, DateTime.Today.AddDays(7),
                DateTime.Today.AddDays(27), EnumLeaveType.Annual);

            await leaveRepository.InsertAsync(leave1, true);
            await leaveRepository.InsertAsync(leave2, true);
        }


        private async Task SeedRoleRecords()
        {
            var doctor = new IdentityRole(id: guidGenerator.Create(), name: "doctor")
            {
                IsPublic = true,
                IsDefault = false
            };

            var staff = new IdentityRole(id: guidGenerator.Create(), name: "staff")
            {
                IsPublic = true,
                IsDefault = false
            };

            await roleManager.CreateAsync(doctor);
            await roleManager.CreateAsync(staff);

            //Doctor permissions
            //Patiens permissions
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Patients.Default, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Patients.Create, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Patients.Delete, true);

            //Doctor permissions
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Doctors.Default, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Doctors.Create, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Doctors.Delete, true);

            //Appointment types permissions
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.AppointmentTypes.Default, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.AppointmentTypes.Create, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.AppointmentTypes.Delete, true);
            
            //Appointment types permissions
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.MedicalServices.Default, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.MedicalServices.Create, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.MedicalServices.Delete, true);
                        
            //Leaves types permissions
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.DoctorLeaves.Default, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.DoctorLeaves.Create, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.DoctorLeaves.Delete, true);
            
            //blood test permissions
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.BloodTests.Default, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.BloodTests.Create, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.BloodTests.Delete, true);
            
            //blood test permissions
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.BloodTestResults.Default, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.BloodTestResults.Create, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.BloodTestResults.Delete, true);
            
            //Departments permissions
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Departments.Default, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Departments.Create, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Departments.Delete, true);
            
            //Treatment permissions
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Treatment.Default, true);
            
            //Icds permissions
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Icds.Default, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Icds.Create, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Icds.Delete, true);
            
            //FamilyHistories permissions
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.FamilyHistories.Default, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.FamilyHistories.Create, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.FamilyHistories.Delete, true);
            
            //Backgrounds permissions
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Backgrounds.Default, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Backgrounds.Create, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Backgrounds.Delete, true);
            
            //PhysicalFindings permissions
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.PhysicalFindings.Default, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.PhysicalFindings.Create, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.PhysicalFindings.Delete, true);
            
            //Examinations permissions
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Examinations.Default, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Examinations.Create, true);
            await permissionManager.SetForRoleAsync(doctor.Name, HealthCarePermissions.Examinations.Delete, true);
            
            /* Staff permissions */
            //Patients permissions
            await permissionManager.SetForRoleAsync(staff.Name, HealthCarePermissions.Patients.Default, true);
            await permissionManager.SetForRoleAsync(staff.Name, HealthCarePermissions.Patients.Create, true);
            await permissionManager.SetForRoleAsync(staff.Name, HealthCarePermissions.Patients.Delete, true);

            //MedicalServices permissions
            await permissionManager.SetForRoleAsync(staff.Name, HealthCarePermissions.MedicalServices.Default, true);
            await permissionManager.SetForRoleAsync(staff.Name, HealthCarePermissions.MedicalServices.Create, true);
            await permissionManager.SetForRoleAsync(staff.Name, HealthCarePermissions.MedicalServices.Delete, true);

            await userManager.CreateAsync(new IdentityUser(guidGenerator.Create(), "doctor@1", "doc1@gmail.com"),
                "doctor@A1");
            await userManager.CreateAsync(new IdentityUser(guidGenerator.Create(), "medicalstaff@1", "medicalstaff1@gmail.com"),
                "medicalstaff@A1");
        }

        private async Task SeedAppointments()
        {
            if (await medicalServiceRepository.GetCountAsync() == 0
                || await patientRepository.GetCountAsync() == 0
                || await doctorRepository.GetCountAsync() == 0
                || await appointmentTypeRepository.GetCountAsync() == 0)
            {
                return;
            }

            var medicalServices = await medicalServiceRepository.GetListAsync();
            var doctors = await doctorRepository.GetListAsync();
            var patients = await patientRepository.GetListAsync();
            var types = await appointmentTypeRepository.GetListAsync();

            var random = new Random();
            var startHour = TimeSpan.FromHours(9);
            var endHour = TimeSpan.FromHours(17);

            var allAppointments = new List<Appointment>();

            foreach (var patient in patients)
            {
                var doctor = doctors[random.Next(doctors.Count)];
                var medicalService = medicalServices[random.Next(medicalServices.Count)];
                var serviceDuration = TimeSpan.FromMinutes(medicalService.Duration);

                var appointmentDate = DateTime.Now.Date.AddDays(1);

                var availableTimes = new List<TimeSpan>();

                for (var time = startHour; time + serviceDuration <= endHour; time += serviceDuration)
                {
                    availableTimes.Add(time);
                }

                var patientAppointments =
                    new HashSet<(DateTime, TimeSpan)>();

                for (var i = 0; i < 10; i++)
                {
                    bool isSlotTaken;
                    TimeSpan appointmentTime;

                    do
                    {
                        appointmentTime = availableTimes[random.Next(availableTimes.Count)];
                        isSlotTaken = patientAppointments.Contains((appointmentDate, appointmentTime));
                    } while (isSlotTaken);

                    patientAppointments.Add((appointmentDate, appointmentTime));

                    try
                    {
                        var appointment = new Appointment(
                            guidGenerator.Create(),
                            doctor.Id,
                            patient.Id,
                            medicalService.Id,
                            types[random.Next(types.Count)].Id,
                            doctor.DepartmentId,
                            appointmentDate,
                            appointmentDate + appointmentTime,
                            appointmentDate + appointmentTime + serviceDuration,
                            (EnumAppointmentStatus)random.Next(0, Enum.GetValues(typeof(EnumAppointmentStatus)).Length),
                            random.NextDouble() > 0.5 ? "Some notes for this appointment" : null,
                            cancellationNotes: null,
                            random.NextDouble() > 0.5,
                            medicalService.Cost
                        );

                        allAppointments.Add(appointment);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Skipping appointment for patient {patient.Id} on {appointmentDate.Add(appointmentTime)}. Error: {ex.Message}");
                    }

                    // Her hasta için 4 farklı randevu saati olmalı
                    if (patientAppointments.Count >= 4)
                    {
                        break;
                    }

                    // Eğer tüm slotlar dolmuşsa bir sonraki gün başlayacak
                    if (appointmentTime + serviceDuration >= endHour)
                    {
                        appointmentDate = appointmentDate.AddDays(1); // Bir sonraki güne geçiş
                        availableTimes.Clear(); // O günün saat dilimlerini sıfırla
                        for (var time = startHour; time + serviceDuration <= endHour; time += serviceDuration)
                        {
                            availableTimes.Add(time); // Yeni güne ait saat dilimlerini oluştur
                        }
                    }
                }
            }

            await appointmentRepository.InsertManyAsync(allAppointments, true);
        }

        private async Task SeedDoctorWorkingHours()
        {
            if (await doctorRepository.GetCountAsync() == 0)
            {
                return;
            }

            var doctors = await doctorRepository.GetListAsync();

            foreach (var doctor in doctors)
            {
                for (var day = (int)DayOfWeek.Monday; day <= (int)DayOfWeek.Friday; day++)
                {
                    var startHour = new TimeOnly(8, 0);
                    var endHour = new TimeOnly(17, 0);

                    var doctorWorkingHour = new DoctorWorkingHour(doctor.Id, (DayOfWeek)day, startHour, endHour);
                    await doctorWorkingHourRepository.InsertAsync(doctorWorkingHour);
                }
            }
        }

        private async Task SeedAppointmentTypes()
        {
            if (await appointmentTypeRepository.GetCountAsync() > 0)
            {
                return;
            }

            await appointmentTypeRepository.InsertAsync(new AppointmentType(guidGenerator.Create(), "Operation"), true);
            await appointmentTypeRepository.InsertAsync(new AppointmentType(guidGenerator.Create(), "General Checkup"),
                true);
            await appointmentTypeRepository.InsertAsync(
                new AppointmentType(guidGenerator.Create(), "Cardiology Consultation"), true);
            await appointmentTypeRepository.InsertAsync(
                new AppointmentType(guidGenerator.Create(), "Pediatrics"), true);
        }

        private async Task SeedIcds()
        {
            var icd1 = new Icd(
                id: Guid.NewGuid(),
                codeNumber: "A01",
                detail: "Typhoid and paratyphoid fevers");
            var icd2 = new Icd(
                id: Guid.NewGuid(),
                codeNumber: "C12",
                detail: "Malignant neoplasm of pyriform sinus");
            var icd3 = new Icd(
                id: Guid.NewGuid(),
                codeNumber: "D78",
                detail: "Intraoperative and postprocedural complications of the spleen");

            await icdRepository.InsertAsync(icd1, true);
            await icdRepository.InsertAsync(icd2, true);
            await icdRepository.InsertAsync(icd3, true);
        }

        private async Task SeedExaminations()
        {
            if (await protocolRepository.GetCountAsync() == 0)
            {
                return;
            }

            var protocols = await protocolRepository.GetListAsync();

            var examination1 = new Examination(
                id: Guid.NewGuid(),
                protocolId: protocols[0].Id,
                date: DateTime.Now,
                complaint: "complaint1",
                startDate: DateTime.Now,
                story: "story1");

            var examination2 = new Examination(
                id: Guid.NewGuid(),
                protocolId: protocols[1].Id,
                date: DateTime.Now,
                complaint: "complaint2",
                startDate: DateTime.Now,
                story: "story2");

            var examination3 = new Examination(
                id: Guid.NewGuid(),
                protocolId: protocols[2].Id,
                date: DateTime.Now,
                complaint: "complaint3",
                startDate: DateTime.Now,
                story: "story3");

            await examinationRepository.InsertAsync(examination1, true);
            await examinationRepository.InsertAsync(examination2, true);
            await examinationRepository.InsertAsync(examination3, true);
        }

        private async Task SeedFamilyHistory()
        {
            if (await examinationRepository.GetCountAsync() == 0)
            {
                return;
            }

            var examinations = await examinationRepository.GetListAsync();

            var familyHistory1 = new FamilyHistory(
                id: Guid.NewGuid(),
                examinationId: examinations[0].Id,
                areParentsRelated: false,
                motherDisease: "Flu",
                fatherDisease: "Diabetes",
                sisterDisease: null,
                brotherDisease: null
            );

            var familyHistory2 = new FamilyHistory(
                id: Guid.NewGuid(),
                examinationId: examinations[1].Id,
                areParentsRelated: false,
                motherDisease: null,
                fatherDisease: "Guatr",
                sisterDisease: null,
                brotherDisease: null
            );
            var familyHistory3 = new FamilyHistory(
                id: Guid.NewGuid(),
                examinationId: examinations[2].Id,
                areParentsRelated: false,
                motherDisease: null,
                fatherDisease: null,
                sisterDisease: null,
                brotherDisease: null
            );

            await familyHistoryRepository.InsertAsync(familyHistory1, true);
            await familyHistoryRepository.InsertAsync(familyHistory2, true);
            await familyHistoryRepository.InsertAsync(familyHistory3, true);
        }

        private async Task SeedBackground()
        {
            if (await examinationRepository.GetCountAsync() == 0)
            {
                return;
            }

            var examinations = await examinationRepository.GetListAsync();

            var background1 = new Background(
                id: Guid.NewGuid(),
                examinationId: examinations[0].Id,
                allergies: "allergy1",
                medications: "medication1",
                habits: "habit1"
            );

            var background2 = new Background(
                id: Guid.NewGuid(),
                examinationId: examinations[1].Id,
                allergies: "allergy2",
                medications: "medication2",
                habits: "habit2"
            );

            var background3 = new Background(
                id: Guid.NewGuid(),
                examinationId: examinations[2].Id,
                allergies: "allergy2",
                medications: "medication2",
                habits: "habit2"
            );

            await backgroundRepository.InsertAsync(background1, true);
            await backgroundRepository.InsertAsync(background2, true);
            await backgroundRepository.InsertAsync(background3, true);
        }

        private async Task SeedPhysicalFindings()
        {
            if (await examinationRepository.GetCountAsync() == 0)
            {
                return;
            }

            var examinations = await examinationRepository.GetListAsync();

            var physicalFinding1 = new PhysicalFinding(
                id: Guid.NewGuid(),
                examinationId: examinations[0].Id,
                weight: 50,
                height: 165,
                bodyTemperature: 37,
                pulse: 100
            );

            var physicalFinding2 = new PhysicalFinding(
                id: Guid.NewGuid(),
                examinationId: examinations[1].Id,
                weight: 50,
                vki: 1,
                kbs: 3,
                spo2: 1
            );

            var physicalFinding3 = new PhysicalFinding(
                id: Guid.NewGuid(),
                examinationId: examinations[2].Id,
                weight: 150,
                height: 195,
                vya: 2,
                kbd: 2
            );

            await physicalFindingRepository.InsertAsync(physicalFinding1, true);
            await physicalFindingRepository.InsertAsync(physicalFinding2, true);
            await physicalFindingRepository.InsertAsync(physicalFinding3, true);
        }
    }
}