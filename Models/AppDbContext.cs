using cdm.Models; // تأكد من تضمين النماذج
using System.Data.Entity;

namespace cdm.Database // أو MVCUnifiedLogin.Database مثلاً
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("AppDbContext") { }
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }

        public DbSet<FacultyMember> FacultyMembers { get; set; }
        public DbSet<College> Colleges { get; set; }
        public DbSet<college_Department> college_Departments { get; set; }
        public DbSet<JobTitle> JobTitles { get; set; }
        public DbSet<UserDepartment> UserDepartments { get; set; }
        public DbSet<StudyLeaveMember> StudyLeaveMembers { get; set; }

        public DbSet<Role> Roles { get; set; }
        public DbSet<DeleteLog> DeleteLogs { get; set; }
        public DbSet<SecondmentsType> SecondmentsTypes { get; set; }
        public DbSet<SecondmentsData> SecondmentsData { get; set; }
        public DbSet<Renewal> Renewals { get; set; }
        public DbSet<Conference> Conferences { get; set; }
        public DbSet<Participation> Participations { get; set; }
        public DbSet<Mission> Missions { get; set; }
        public DbSet<ScientificMission> ScientificMissions { get; set; }

        public DbSet<VisitingProfessor> VisitingProfessors { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SecondmentsData>().ToTable("SecondmentsData");

            base.OnModelCreating(modelBuilder);
        }

    }
}