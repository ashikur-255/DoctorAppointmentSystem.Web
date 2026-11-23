namespace DoctorAppointmentSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatePatientModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Patients", "Age", c => c.Int());
            AddColumn("dbo.Patients", "Gender", c => c.String(maxLength: 10));
            AddColumn("dbo.Patients", "BloodGroup", c => c.String());
            AddColumn("dbo.Patients", "Address", c => c.String(maxLength: 200));
            AddColumn("dbo.Patients", "DiseaseDetails", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Patients", "DiseaseDetails");
            DropColumn("dbo.Patients", "Address");
            DropColumn("dbo.Patients", "BloodGroup");
            DropColumn("dbo.Patients", "Gender");
            DropColumn("dbo.Patients", "Age");
        }
    }
}
