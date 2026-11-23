namespace DoctorAppointmentSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixModelIssue : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Doctors", "Email", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Doctors", "Email");
        }
    }
}
