namespace DoctorAppointmentSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixDoctorAppointmentModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "IsVisited", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "IsVisited");
        }
    }
}
