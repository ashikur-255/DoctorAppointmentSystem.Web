namespace DoctorAppointmentSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDoctorDetails1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Doctors", "VisitedTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Doctors", "VisitedTime", c => c.String(nullable: false));
        }
    }
}
