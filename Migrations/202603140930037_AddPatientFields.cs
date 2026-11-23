namespace DoctorAppointmentSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPatientFields : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Patients", "Email", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Patients", "Email", c => c.String());
        }
    }
}
