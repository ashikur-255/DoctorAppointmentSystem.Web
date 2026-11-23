namespace DoctorAppointmentSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDoctorFeeAndPaymentIsPaid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Doctors", "Fee", c => c.Int(nullable: false));
            AddColumn("dbo.Payments", "IsPaid", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Payments", "IsPaid");
            DropColumn("dbo.Doctors", "Fee");
        }
    }
}
