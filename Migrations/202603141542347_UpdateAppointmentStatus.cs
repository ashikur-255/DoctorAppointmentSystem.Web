namespace DoctorAppointmentSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateAppointmentStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "Status");
        }
    }
}
