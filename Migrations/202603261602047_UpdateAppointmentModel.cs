namespace DoctorAppointmentSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateAppointmentModel : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Patients", "Age", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Patients", "Age", c => c.Int());
        }
    }
}
