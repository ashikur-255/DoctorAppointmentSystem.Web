namespace DoctorAppointmentSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDoctorDetails2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Doctors", "VisitedTime", c => c.Time(precision: 7));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Doctors", "VisitedTime", c => c.DateTime(nullable: false));
        }
    }
}
