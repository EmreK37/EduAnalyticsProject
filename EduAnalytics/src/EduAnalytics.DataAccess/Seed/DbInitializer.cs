using EduAnalytics.Core.Entities;
using EduAnalytics.Core.Enums;
using EduAnalytics.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace EduAnalytics.DataAccess.Seed;

/// <summary>
/// Veritabanını test verisiyle doldurur.
/// Analiz algoritmasını doğrulamak için senaryolaştırılmış veriler içerir.
///
/// Önemli Senaryolar:
///   Soru 4 → Doğru: A. Yanlış yapan 18 kişiden 15'i C şıkkını seçti (%83 çeldirici).
///   Soru 7 → Doğru: D. Başarı oranı ~%28 (konu zayıflığı tespiti).
/// </summary>
public static class DbInitializer
{
    public static void Seed(EduAnalyticsDbContext context)
    {
        // MVP için: Migration kullanmadan modele göre veritabanını oluştur.
        // İleride şemada değişiklik olursa Migration yapısına geçilecek.
        
        // Yeniden seed tetiklendiğinde eski veritabanını temizlemesi için EnsureDeleted eklendi.
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        if (context.Users.Any())
            return; // Zaten seed yapılmış
            
        // ═══════════════════════════════════════════
        // 1) KULLANICI
        // ═══════════════════════════════════════════
        var teacher = new User
        {
            FullName = "Dr. Eren Aksoy",
            Email = "eren.aksoy@edu.tr",
            PasswordHash = "SHA256_PLACEHOLDER_NOT_REAL",
            Role = UserRole.Teacher,
            CreatedAt = new DateTime(2025, 9, 1, 8, 0, 0, DateTimeKind.Utc)
        };
        context.Users.Add(teacher);
        context.SaveChanges();

        // ═══════════════════════════════════════════
        // 2) DERS
        // ═══════════════════════════════════════════
        var course = new Course
        {
            Name = "Yazılım Kalite ve Güvencesi",
            Code = "SQA101",
            Description = "Yazılım test süreçleri, kalite metrikleri ve otomasyon araçlarını kapsayan ders.",
            CreatedByUserId = teacher.Id
        };
        
        var sweCourse = new Course { Name = "Yazılım Mühendisliğine Giriş", Code = "SWE101", Description = "Yazılım mühendisliği prensipleri", CreatedByUserId = teacher.Id };
        var algCourse = new Course { Name = "Veri Yapıları ve Algoritmalar", Code = "CS201", Description = "Temel veri yapıları", CreatedByUserId = teacher.Id };
        var dbCourse = new Course { Name = "Veritabanı Yönetim Sistemleri", Code = "DB301", Description = "İlişkisel veritabanı tasarımı", CreatedByUserId = teacher.Id };
        var webCourse = new Course { Name = "Web Programlama", Code = "WEB401", Description = "Modern web teknolojileri", CreatedByUserId = teacher.Id };
        var archCourse = new Course { Name = "Yazılım Mimarisi ve Tasarım Şablonları", Code = "ARC501", Description = "Yazılım mimarisi stilleri", CreatedByUserId = teacher.Id };

        context.Courses.AddRange(course, sweCourse, algCourse, dbCourse, webCourse, archCourse);
        context.SaveChanges();

        // ═══════════════════════════════════════════
        // 2.5) ÖĞRENİM ÇIKTILARI (20 Adet Havuz)
        // ═══════════════════════════════════════════
        var outcomes = new List<LearningOutcome>();
        for (int i = 1; i <= 20; i++)
        {
            outcomes.Add(new LearningOutcome
            {
                Name = $"Öğrenim Çıktısı {i}",
                Description = $"Genel öğrenim çıktısı tanımlaması {i}. Öğrenci bu hedefi başarıyla tamamlayabilir."
            });
        }
        context.LearningOutcomes.AddRange(outcomes);
        context.SaveChanges();

        // ═══════════════════════════════════════════
        // 3) KONULAR (Bologna Haftalık Çıktılar)
        // ═══════════════════════════════════════════
        var topic1 = new Topic
        {
            CourseId = course.Id,
            WeekNumber = 3,
            Title = "Test Seviyeleri",
            Description = "Birim, entegrasyon, sistem ve kabul testleri",
            LearningOutcome = "Öğrenci, farklı test seviyelerini tanımlayabilir ve uygun senaryolarda kullanabilir."
        };
        var topic2 = new Topic
        {
            CourseId = course.Id,
            WeekNumber = 5,
            Title = "Statik Analiz Teknikleri",
            Description = "Kod inceleme, statik analiz araçları, metrik tabanlı kalite ölçümü",
            LearningOutcome = "Öğrenci, statik analiz araçlarını kullanarak kod kalitesini değerlendirebilir."
        };
        var topic3 = new Topic
        {
            CourseId = course.Id,
            WeekNumber = 8,
            Title = "Test Otomasyon Araçları",
            Description = "Selenium, xUnit, NUnit ve CI/CD entegrasyonu",
            LearningOutcome = "Öğrenci, test otomasyon frameworkleri ile tekrarlanabilir testler yazabilir."
        };
        context.Topics.AddRange(topic1, topic2, topic3);
        
        string[] sweTopicTitles = ["Yazılım Geliştirme Yaşam Döngüsü (SDLC)", "Çevik (Agile) Metodolojiler ve Scrum", "Gereksinim Mühendisliği", "Sistem Modelleme ve UML", "Yazılım Mimarisi Temelleri", "UI ve UX Tasarımı", "Yazılım Geliştirme Standartları", "Yazılım Test Stratejileri", "Yazılım Bakımı ve Evrimi", "Proje Yönetimi ve Planlama", "Risk Yönetimi", "Kalite Güvencesi ve Metrikler", "Konfigürasyon Yönetimi (Git)", "Profesyonel Sorumluluk ve Etik"];
        for (int i = 0; i < 14; i++) context.Topics.Add(new Topic { CourseId = sweCourse.Id, WeekNumber = i + 1, Title = sweTopicTitles[i], Description = $"{sweTopicTitles[i]} konularının temel prensipleri", LearningOutcome = $"Öğrenci {sweTopicTitles[i].ToLower()} hakkında bilgi sahibi olur." });
        
        string[] algTopicTitles = ["Algoritma Analizi ve Big O", "Diziler ve Bağlı Listeler", "Yığınlar (Stacks) ve Kuyruklar (Queues)", "İkili Arama Ağaçları (BST)", "AVL ve Kırmızı-Siyah Ağaçlar", "B-Ağaçları ve Trie", "Öncelik Kuyrukları ve Heap", "Hash Tabloları", "Çizge (Graph) Teorisine Giriş", "Çizge Gezinme (BFS, DFS)", "En Kısa Yol Algoritmaları", "Minimum Kaplayan Ağaç (MST)", "Sıralama Algoritmaları", "Dinamik Programlama Temelleri"];
        for (int i = 0; i < 14; i++) context.Topics.Add(new Topic { CourseId = algCourse.Id, WeekNumber = i + 1, Title = algTopicTitles[i], Description = $"{algTopicTitles[i]} mantığının anlaşılması", LearningOutcome = $"Öğrenci {algTopicTitles[i].ToLower()} yapısını uygulayabilir." });
        
        string[] dbTopicTitles = ["Veritabanı Sistemlerine Giriş", "Varlık-İlişki (ER) Modeli", "İlişkisel Veri Modeli ve Cebiri", "Temel SQL Sorguları", "İleri Düzey SQL", "Tasarım ve Normalizasyon", "İndeksleme ve Hashing", "Sorgu İşleme ve Optimizasyon", "Transaction ve ACID", "Eşzamanlılık (Concurrency) Kontrolü", "Kurtarma (Recovery) Teknikleri", "NoSQL Veritabanlarına Giriş", "Dağıtık Veritabanı Sistemleri", "Veritabanı Güvenliği"];
        for (int i = 0; i < 14; i++) context.Topics.Add(new Topic { CourseId = dbCourse.Id, WeekNumber = i + 1, Title = dbTopicTitles[i], Description = $"{dbTopicTitles[i]} üzerine kavramsal çalışmalar", LearningOutcome = $"Öğrenci {dbTopicTitles[i].ToLower()} hakkında yetkinlik kazanır." });
        
        string[] webTopicTitles = ["Web Mimarisi ve HTTP", "HTML5 ve Semantik Web", "CSS3 ve Modern Düzenler", "JavaScript Temelleri", "DOM Manipülasyonu ve Olaylar", "Asenkron JavaScript (Fetch, Promises)", "SPA ve Modern Frameworklere Giriş", "Durum (State) Yönetimi", "Backend Servislerine Giriş", "RESTful API Mimarisi", "Kimlik Doğrulama: JWT", "Web Güvenliği (CORS, XSS, CSRF)", "ORM ve Veritabanı Entegrasyonu", "Uygulama Dağıtımı (Deployment)"];
        for (int i = 0; i < 14; i++) context.Topics.Add(new Topic { CourseId = webCourse.Id, WeekNumber = i + 1, Title = webTopicTitles[i], Description = $"{webTopicTitles[i]} geliştirme yöntemleri", LearningOutcome = $"Öğrenci {webTopicTitles[i].ToLower()} teknikleriyle uygulama geliştirebilir." });
        
        string[] archTopicTitles = ["Yazılım Mimarisinin Temelleri", "SOLID Prensipleri", "Katmanlı ve Monolitik Mimari", "N-Tier ve İstemci-Sunucu", "Mikroservis Mimarisine Giriş", "Olay Güdümlü (Event-Driven) Mimari", "Yaratımsal Tasarım Şablonları", "İleri Yaratımsal Şablonlar", "Yapısal Tasarım Şablonları", "İleri Yapısal Şablonlar", "Davranışsal Tasarım Şablonları", "İleri Davranışsal Şablonlar", "CQRS ve Event Sourcing", "Clean ve Hexagonal Mimari"];
        for (int i = 0; i < 14; i++) context.Topics.Add(new Topic { CourseId = archCourse.Id, WeekNumber = i + 1, Title = archTopicTitles[i], Description = $"{archTopicTitles[i]} teorisi ve pratiği", LearningOutcome = $"Öğrenci {archTopicTitles[i].ToLower()} standartlarını uygulayabilir." });

        context.SaveChanges();

        // ═══════════════════════════════════════════
        // 4) SINAV
        // ═══════════════════════════════════════════
        var exam = new Exam
        {
            CourseId = course.Id,
            Title = "SQA101 Vize Sınavı",
            ExamDate = new DateTime(2025, 11, 15, 10, 0, 0, DateTimeKind.Utc),
            TotalQuestions = 12,  // 10 test + 2 klasik
            CreatedByUserId = teacher.Id
        };
        context.Exams.Add(exam);
        context.SaveChanges();

        // ═══════════════════════════════════════════
        // 5) SORULAR (10 Adet)
        // ═══════════════════════════════════════════
        var questions = new List<Question>
        {
            new()
            {
                ExamId = exam.Id, QuestionNumber = 1,
                QuestionText = "Birim testi (unit test) hangi seviyede yapılır?",
                OptionA = "Sistem seviyesinde",
                OptionB = "Fonksiyon/metot seviyesinde",
                OptionC = "Kabul testi seviyesinde",
                OptionD = "Entegrasyon seviyesinde",
                CorrectOption = OptionLetter.B
            },
            new()
            {
                ExamId = exam.Id, QuestionNumber = 2,
                QuestionText = "Entegrasyon testinin temel amacı nedir?",
                OptionA = "Kullanıcı arayüzünü test etmek",
                OptionB = "Tek bir fonksiyonun doğruluğunu kontrol etmek",
                OptionC = "Performans ölçümü yapmak",
                OptionD = "Modüller arası etkileşimleri doğrulamak",
                CorrectOption = OptionLetter.D
            },
            new()
            {
                // Soru 3: 2 konuya bağlı (Topic 1 + Topic 2)
                ExamId = exam.Id, QuestionNumber = 3,
                QuestionText = "Statik analiz ile tespit edilemeyen ancak birim testleriyle bulunabilecek hata türü hangisidir?",
                OptionA = "Çalışma zamanı (runtime) mantık hataları",
                OptionB = "Kod standart ihlalleri",
                OptionC = "Kullanılmayan değişkenler",
                OptionD = "Yanlış isimlendirme kalıpları",
                CorrectOption = OptionLetter.A
            },
            new()
            {
                // Soru 4: ÇELDİRİCİ SENARYOSU — Doğru: A, Çeldirici: C
                ExamId = exam.Id, QuestionNumber = 4,
                QuestionText = "Cyclomatic Complexity (Döngüsel Karmaşıklık) metriği neyi ölçer?",
                OptionA = "Koddaki bağımsız yol sayısını",
                OptionB = "Satır başına hata oranını",
                OptionC = "Kod tekrar (duplication) oranını",
                OptionD = "Fonksiyon çağrı derinliğini",
                CorrectOption = OptionLetter.A
            },
            new()
            {
                ExamId = exam.Id, QuestionNumber = 5,
                QuestionText = "SonarQube hangi amaçla kullanılır?",
                OptionA = "Performans testi",
                OptionB = "Yük testi",
                OptionC = "Statik kod analizi ve kalite ölçümü",
                OptionD = "Veritabanı testi",
                CorrectOption = OptionLetter.C
            },
            new()
            {
                ExamId = exam.Id, QuestionNumber = 6,
                QuestionText = "Selenium aracı öncelikli olarak ne için kullanılır?",
                OptionA = "API testi",
                OptionB = "Web tarayıcı tabanlı otomasyon testi",
                OptionC = "Birim testi",
                OptionD = "Güvenlik testi",
                CorrectOption = OptionLetter.B
            },
            new()
            {
                // Soru 7: 2 konuya bağlı (Topic 1 + Topic 3) — DÜŞÜK BAŞARI
                ExamId = exam.Id, QuestionNumber = 7,
                QuestionText = "Regresyon testlerinin CI/CD pipeline'ına entegre edilmesinin en önemli faydası nedir?",
                OptionA = "Geliştirme maliyetini düşürür",
                OptionB = "Kod satır sayısını azaltır",
                OptionC = "Manuel test ihtiyacını tamamen ortadan kaldırır",
                OptionD = "Her değişiklikte mevcut fonksiyonların bozulmadığını otomatik doğrular",
                CorrectOption = OptionLetter.D
            },
            new()
            {
                ExamId = exam.Id, QuestionNumber = 8,
                QuestionText = "xUnit framework'ünde [Fact] attribute'ü ne anlama gelir?",
                OptionA = "Parametresiz, tek bir test vakasını tanımlar",
                OptionB = "Parametreli test vakası tanımlar",
                OptionC = "Test sınıfını işaretler",
                OptionD = "Test sonucunu loglar",
                CorrectOption = OptionLetter.A
            },
            new()
            {
                // Soru 9: 2 konuya bağlı (Topic 2 + Topic 3)
                ExamId = exam.Id, QuestionNumber = 9,
                QuestionText = "Kod kapsama (code coverage) oranı %100 olduğunda ne söylenebilir?",
                OptionA = "Yazılımda hiç hata yoktur",
                OptionB = "Tüm kullanıcı senaryoları test edilmiştir",
                OptionC = "Tüm kod satırları en az bir kez çalıştırılmıştır ancak hatasızlık garanti değildir",
                OptionD = "Performans testleri tamamlanmıştır",
                CorrectOption = OptionLetter.C
            },
            new()
            {
                ExamId = exam.Id, QuestionNumber = 10,
                QuestionText = "Kabul testi (acceptance test) kimler tarafından yürütülür?",
                OptionA = "Sadece yazılım geliştiriciler",
                OptionB = "Son kullanıcılar veya müşteri temsilcileri",
                OptionC = "Sadece test mühendisleri",
                OptionD = "Veritabanı yöneticileri",
                CorrectOption = OptionLetter.B
            },
            // ══ KLASİK SORULAR ══
            new()
            {
                ExamId = exam.Id, QuestionNumber = 11,
                Type = QuestionType.OpenEnded,
                MaxPoints = 10.0m,
                QuestionText = "Birim testi, entegrasyon testi ve sistem testinin farklarını açıklayınız. " +
                               "Her biri için bir örnek veriniz.",
                AnswerKey = "Birim: tek metot izole test. Entegrasyon: modüllerin birlikte çalışması. " +
                            "Sistem: uçtan uca. Her biri için örnek verilmiş olmalı.",
                CorrectOption = OptionLetter.Empty
            },
            new()
            {
                ExamId = exam.Id, QuestionNumber = 12,
                Type = QuestionType.OpenEnded,
                MaxPoints = 15.0m,
                QuestionText = "Bir e-ticaret uygulamasında kod kapsama oranınız %95 olmasına rağmen " +
                               "üretim ortamında kritik bir hata ile karşılaştınız. Bu durumu nasıl " +
                               "yorumlarsınız? Kapsam metriğinin sınırlılıkları nelerdir?",
                AnswerKey = "Coverage ≠ kalite. Satırın çalıştırılması doğruluğu garanti etmez. " +
                            "Edge case'ler, veri kombinasyonları test edilmemiş olabilir. " +
                            "Mutation testing / property-based testing önerilir.",
                CorrectOption = OptionLetter.Empty
            }
        };
        context.Questions.AddRange(questions);
        context.SaveChanges();

        // ═══════════════════════════════════════════
        // 6) SORU-KONU İLİŞKİLERİ (Many-to-Many)
        // ═══════════════════════════════════════════
        // Q1 → Topic1, Q2 → Topic1
        // Q3 → Topic1 + Topic2 (çoklu)
        // Q4 → Topic2, Q5 → Topic2
        // Q6 → Topic3
        // Q7 → Topic1 + Topic3 (çoklu)
        // Q8 → Topic3
        // Q9 → Topic2 + Topic3 (çoklu)
        // Q10 → Topic1
        var questionTopics = new List<QuestionTopic>
        {
            new() { QuestionId = questions[0].Id, TopicId = topic1.Id },  // Q1 → T1
            new() { QuestionId = questions[1].Id, TopicId = topic1.Id },  // Q2 → T1
            new() { QuestionId = questions[2].Id, TopicId = topic1.Id },  // Q3 → T1
            new() { QuestionId = questions[2].Id, TopicId = topic2.Id },  // Q3 → T2 (çoklu)
            new() { QuestionId = questions[3].Id, TopicId = topic2.Id },  // Q4 → T2
            new() { QuestionId = questions[4].Id, TopicId = topic2.Id },  // Q5 → T2
            new() { QuestionId = questions[5].Id, TopicId = topic3.Id },  // Q6 → T3
            new() { QuestionId = questions[6].Id, TopicId = topic1.Id },  // Q7 → T1
            new() { QuestionId = questions[6].Id, TopicId = topic3.Id },  // Q7 → T3 (çoklu)
            new() { QuestionId = questions[7].Id, TopicId = topic3.Id },  // Q8 → T3
            new() { QuestionId = questions[8].Id, TopicId = topic2.Id },  // Q9 → T2
            new() { QuestionId = questions[8].Id, TopicId = topic3.Id },  // Q9 → T3 (çoklu)
            new() { QuestionId = questions[9].Id, TopicId = topic1.Id },  // Q10 → T1
            new() { QuestionId = questions[10].Id, TopicId = topic1.Id }, // Q11 (klasik) → T1
            new() { QuestionId = questions[11].Id, TopicId = topic2.Id }, // Q12 (klasik) → T2
        };
        context.QuestionTopics.AddRange(questionTopics);
        context.SaveChanges();

        // ═══════════════════════════════════════════
        // 6.5) SORU-ÖĞRENİM ÇIKTISI İLİŞKİLERİ (Many-to-Many)
        // ═══════════════════════════════════════════
        var questionOutcomes = new List<QuestionLearningOutcome>
        {
            new() { QuestionId = questions[0].Id, LearningOutcomeId = outcomes[0].Id },
            new() { QuestionId = questions[1].Id, LearningOutcomeId = outcomes[1].Id },
            new() { QuestionId = questions[2].Id, LearningOutcomeId = outcomes[2].Id },
            new() { QuestionId = questions[3].Id, LearningOutcomeId = outcomes[3].Id },
            new() { QuestionId = questions[4].Id, LearningOutcomeId = outcomes[4].Id },
            new() { QuestionId = questions[4].Id, LearningOutcomeId = outcomes[5].Id }, // Çoklu
            new() { QuestionId = questions[5].Id, LearningOutcomeId = outcomes[6].Id },
            new() { QuestionId = questions[6].Id, LearningOutcomeId = outcomes[7].Id },
            new() { QuestionId = questions[7].Id, LearningOutcomeId = outcomes[8].Id },
            new() { QuestionId = questions[8].Id, LearningOutcomeId = outcomes[9].Id },
            new() { QuestionId = questions[9].Id, LearningOutcomeId = outcomes[10].Id }
        };
        context.QuestionLearningOutcomes.AddRange(questionOutcomes);
        context.SaveChanges();

        // ═══════════════════════════════════════════
        // 7) ÖĞRENCİLER (25 Kişi)
        // ═══════════════════════════════════════════
        var students = new List<Student>
        {
            // Cok farkli bölümlerden ogrenciler - Karma sinif
            new() { StudentNumber = "2024001", FullName = "Ahmet Yılmaz",     ClassName = "Yazılım - 3.Sınıf" },
            new() { StudentNumber = "2024002", FullName = "Ayşe Kaya",        ClassName = "Bilgisayar - 4.Sınıf" },
            new() { StudentNumber = "2024003", FullName = "Mehmet Demir",     ClassName = "Yapay Zeka - 2.Sınıf" },
            new() { StudentNumber = "2024004", FullName = "Fatma Çelik",      ClassName = "Yazılım - 4.Sınıf" },
            new() { StudentNumber = "2024005", FullName = "Ali Şahin",        ClassName = "YBS - 3.Sınıf" },
            new() { StudentNumber = "2024006", FullName = "Zeynep Yıldız",    ClassName = "Elektrik - 4.Sınıf" },
            new() { StudentNumber = "2024007", FullName = "Mustafa Özdemir",  ClassName = "Bilgisayar - 3.Sınıf" },
            new() { StudentNumber = "2024008", FullName = "Elif Arslan",      ClassName = "Yazılım - 3.Sınıf" },
            new() { StudentNumber = "2024009", FullName = "Hasan Doğan",      ClassName = "Yazılım - 2.Sınıf" },
            new() { StudentNumber = "2024010", FullName = "Merve Kılıç",      ClassName = "YBS - 4.Sınıf" },
            new() { StudentNumber = "2024011", FullName = "Hüseyin Aydın",    ClassName = "Yapay Zeka - 3.Sınıf" },
            new() { StudentNumber = "2024012", FullName = "Büşra Öztürk",     ClassName = "Bilgisayar - 2.Sınıf" },
            new() { StudentNumber = "2024013", FullName = "İbrahim Çetin",    ClassName = "Yazılım - 3.Sınıf" },
            new() { StudentNumber = "2024014", FullName = "Seda Koç",         ClassName = "Elektrik - 3.Sınıf" },
            new() { StudentNumber = "2024015", FullName = "Emre Kara",        ClassName = "Yazılım - 4.Sınıf" },
            new() { StudentNumber = "2024016", FullName = "Gizem Aksoy",      ClassName = "Bilgisayar - 4.Sınıf" },
            new() { StudentNumber = "2024017", FullName = "Burak Polat",      ClassName = "YBS - 2.Sınıf" },
            new() { StudentNumber = "2024018", FullName = "Esra Erdoğan",     ClassName = "Yapay Zeka - 4.Sınıf" },
            new() { StudentNumber = "2024019", FullName = "Oğuz Tekin",       ClassName = "Bilgisayar - 3.Sınıf" },
            new() { StudentNumber = "2024020", FullName = "Gamze Güneş",      ClassName = "Yazılım - 2.Sınıf" },
            new() { StudentNumber = "2024021", FullName = "Cem Korkmaz",      ClassName = "Elektrik - 4.Sınıf" },
            new() { StudentNumber = "2024022", FullName = "Derya Yılmazer",   ClassName = "Yazılım - 3.Sınıf" },
            new() { StudentNumber = "2024023", FullName = "Serkan Aktaş",     ClassName = "Bilgisayar - 3.Sınıf" },
            new() { StudentNumber = "2024024", FullName = "Tuğba Şen",        ClassName = "YBS - 3.Sınıf" },
            new() { StudentNumber = "2024025", FullName = "Furkan Başaran",   ClassName = "Yazılım - 4.Sınıf" },
        };
        context.Students.AddRange(students);
        context.SaveChanges();

        // ═══════════════════════════════════════════
        // 7.5) ÖĞRENCİ - DERS KAYITLARI (StudentCourse)
        // ═══════════════════════════════════════════
        // Tüm bu 25 öğrenci, bölümleri ve sınıfları ne olursa olsun SQA101 dersine kayıtlılar (CourseId = course.Id)
        var studentCourses = students.Select(s => new StudentCourse
        {
            StudentId = s.Id,
            CourseId = course.Id
        }).ToList();

        context.StudentCourses.AddRange(studentCourses);
        context.SaveChanges();

        // ═══════════════════════════════════════════
        // 8) ÖĞRENCİ CEVAPLARI (25 x 10 = 250 Kayıt)
        // ═══════════════════════════════════════════
        //
        // Her soru için öğrencilerin seçtiği şıkları belirleyen matris.
        // Matrisin her satırı bir soruyu, her sütunu bir öğrenciyi temsil eder.
        //
        // Doğru cevaplar: Q1=B, Q2=D, Q3=A, Q4=A, Q5=C, Q6=B, Q7=D, Q8=A, Q9=C, Q10=B
        //
        // ÇELDİRİCİ SENARYO (Soru 4):
        //   Doğru=A → 7 doğru, 18 yanlış.
        //   Yanlış yapan 18 kişiden 15'i C seçti (çeldirici oranı: %83)
        //
        // DÜŞÜK BAŞARI SENARYO (Soru 7):
        //   Doğru=D → 7 doğru, 18 yanlış.
        //   Yanlışlar eşit dağılmış: A=6, B=5, C=7

        // Satırlar: soru indeksi (0-9), Sütunlar: öğrenci indeksi (0-24)
        // Değerler: OptionLetter enum
        var A = OptionLetter.A;
        var B = OptionLetter.B;
        var C = OptionLetter.C;
        var D = OptionLetter.D;

        OptionLetter[][] answerMatrix =
        [
            //                S01 S02 S03 S04 S05 S06 S07 S08 S09 S10 S11 S12 S13 S14 S15 S16 S17 S18 S19 S20 S21 S22 S23 S24 S25
            // Q1  Doğru: B   18 doğru, 7 yanlış (A=3, C=2, D=2)    → %72 başarı
            /*Q1*/  [ B,  B,  A,  B,  B,  B,  C,  B,  B,  A,  B,  B,  D,  B,  B,  A,  B,  B,  C,  B,  D,  B,  B,  B,  B ],

            // Q2  Doğru: D   16 doğru, 9 yanlış (A=3, B=4, C=2)    → %64 başarı
            /*Q2*/  [ D,  B,  D,  D,  A,  D,  D,  B,  D,  D,  C,  D,  A,  B,  D,  D,  B,  D,  D,  A,  C,  D,  D,  D,  D ],

            // Q3  Doğru: A   14 doğru, 11 yanlış (B=5, C=3, D=3)   → %56 başarı
            /*Q3*/  [ A,  B,  A,  A,  D,  A,  B,  C,  A,  A,  B,  A,  D,  C,  A,  B,  A,  A,  D,  A,  C,  B,  A,  A,  A ],

            // Q4  Doğru: A   7 doğru, 18 yanlış — ÇELDİRİCİ: C=15, B=2, D=1   → %28 başarı
            /*Q4*/  [ C,  C,  A,  C,  C,  C,  A,  C,  C,  B,  C,  A,  C,  C,  C,  A,  D,  C,  C,  A,  B,  C,  A,  C,  A ],

            // Q5  Doğru: C   17 doğru, 8 yanlış (A=3, B=3, D=2)    → %68 başarı
            /*Q5*/  [ C,  C,  A,  C,  C,  B,  C,  C,  D,  C,  C,  A,  C,  B,  C,  C,  C,  A,  D,  C,  B,  C,  C,  C,  C ],

            // Q6  Doğru: B   15 doğru, 10 yanlış (A=4, C=3, D=3)   → %60 başarı
            /*Q6*/  [ B,  A,  B,  C,  B,  B,  D,  A,  B,  B,  C,  B,  A,  B,  D,  B,  B,  C,  B,  A,  D,  B,  B,  B,  B ],

            // Q7  Doğru: D   7 doğru, 18 yanlış (A=6, B=5, C=7)    → %28 başarı
            /*Q7*/  [ A,  C,  C,  D,  B,  A,  C,  B,  A,  C,  D,  B,  A,  C,  D,  A,  B,  C,  D,  C,  A,  B,  D,  D,  D ],

            // Q8  Doğru: A   19 doğru, 6 yanlış (B=2, C=2, D=2)    → %76 başarı
            /*Q8*/  [ A,  A,  A,  B,  A,  A,  A,  C,  A,  A,  A,  A,  D,  A,  A,  A,  B,  A,  A,  C,  A,  D,  A,  A,  A ],

            // Q9  Doğru: C   13 doğru, 12 yanlış (A=4, B=5, D=3)   → %52 başarı
            /*Q9*/  [ C,  B,  A,  C,  C,  B,  D,  A,  C,  C,  B,  C,  A,  B,  C,  D,  C,  C,  B,  A,  C,  D,  C,  C,  C ],

            // Q10 Doğru: B   20 doğru, 5 yanlış (A=2, C=2, D=1)    → %80 başarı
            /*Q10*/ [ B,  B,  B,  A,  B,  B,  B,  B,  C,  B,  B,  B,  A,  B,  B,  B,  B,  D,  B,  B,  C,  B,  B,  B,  B ],
        ];

        OptionLetter[] correctAnswers = [B, D, A, A, C, B, D, A, C, B];

        var studentAnswers = new List<StudentAnswer>();

        // 10 test sorusu için cevaplar
        for (int qi = 0; qi < 10; qi++)
        {
            for (int si = 0; si < 25; si++)
            {
                var selected = answerMatrix[qi][si];
                studentAnswers.Add(new StudentAnswer
                {
                    QuestionId = questions[qi].Id,
                    StudentId = students[si].Id,
                    SelectedOption = selected,
                    IsCorrect = selected == correctAnswers[qi],
                    Score = null  // Test sorularında Score null
                });
            }
        }

        // Klasik sorular için öğretmen puanları (0 ile MaxPoints arası dağılım)
        // Q11 MaxPoints=10, ortalama ~6 puan (orta başarı)
        decimal[] q11Scores = [8, 9, 4, 10, 3, 7, 6, 5, 9, 6, 4, 8, 2, 5, 7, 9, 6, 3, 8, 5, 4, 7, 10, 6, 5];
        // Q12 MaxPoints=15, ortalama ~9 puan (zor soru)
        decimal[] q12Scores = [13, 11, 5, 14, 2, 10, 8, 6, 12, 9, 4, 11, 1, 7, 10, 13, 8, 4, 11, 7, 5, 9, 15, 8, 7];

        for (int si = 0; si < 25; si++)
        {
            var q11Score = q11Scores[si];
            studentAnswers.Add(new StudentAnswer
            {
                QuestionId = questions[10].Id,
                StudentId = students[si].Id,
                SelectedOption = OptionLetter.Empty,
                Score = q11Score,
                IsCorrect = q11Score >= 5  // MaxPoints/2 eşiği
            });

            var q12Score = q12Scores[si];
            studentAnswers.Add(new StudentAnswer
            {
                QuestionId = questions[11].Id,
                StudentId = students[si].Id,
                SelectedOption = OptionLetter.Empty,
                Score = q12Score,
                IsCorrect = q12Score >= 7.5m  // MaxPoints/2 eşiği
            });
        }

        context.StudentAnswers.AddRange(studentAnswers);
        context.SaveChanges();
    }
}
