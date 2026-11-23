namespace DoctorAppointmentSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabaseSchema : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Doctors", "Fee");
            DropColumn("dbo.Doctors", "Experience");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Doctors", "Experience", c => c.String());
            AddColumn("dbo.Doctors", "Fee", c => c.Int(nullable: false));
        }
    }
}
