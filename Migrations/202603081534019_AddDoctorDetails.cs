namespace DoctorAppointmentSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDoctorDetails : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Doctors", "HospitalName", c => c.String(nullable: false, maxLength: 200));
            AddColumn("dbo.Doctors", "RegistrationNo", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.Doctors", "Degree", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Doctors", "VisitedTime", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Doctors", "VisitedTime");
            DropColumn("dbo.Doctors", "Degree");
            DropColumn("dbo.Doctors", "RegistrationNo");
            DropColumn("dbo.Doctors", "HospitalName");
        }
    }
}
