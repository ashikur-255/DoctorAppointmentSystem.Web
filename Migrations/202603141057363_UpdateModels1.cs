namespace DoctorAppointmentSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateModels1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Doctors", "Experience", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Doctors", "Experience");
        }
    }
}
