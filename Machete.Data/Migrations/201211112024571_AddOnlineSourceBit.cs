namespace Machete.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOnlineSourceBit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WorkOrders", "onlineSource", c => c.Boolean());
            AddColumn("dbo.Employers", "onlineSource", c => c.Boolean());
            AddColumn("dbo.Employers", "returnCustomer", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Employers", "onlineSource");
            DropColumn("dbo.WorkOrders", "onlineSource");
            DropColumn("dbo.WorkOrders", "returnCustomer");
        }
    }
}