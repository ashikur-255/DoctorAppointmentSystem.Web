namespace DoctorAppointmentSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateModels : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Doctors", "Name", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Doctors", "Name", c => c.String(nullable: false));
        }
    }
}
