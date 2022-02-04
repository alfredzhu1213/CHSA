
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 05/16/2018 10:16:05
-- Generated from EDMX file: C:\Users\Jon Hardin\Documents\Visual Studio 2015\Projects\Clarity HSA\Clarity HSA\Models\ClarityContext.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [Clarity];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_UserRole]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Roles] DROP CONSTRAINT [FK_UserRole];
GO
IF OBJECT_ID(N'[dbo].[FK_OrganizationRole]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Roles] DROP CONSTRAINT [FK_OrganizationRole];
GO
IF OBJECT_ID(N'[dbo].[FK_OrganizationOrganization]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Organizations] DROP CONSTRAINT [FK_OrganizationOrganization];
GO
IF OBJECT_ID(N'[dbo].[FK_EmployeeUser]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Users] DROP CONSTRAINT [FK_EmployeeUser];
GO
IF OBJECT_ID(N'[dbo].[FK_EmployeeLoan]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Charges] DROP CONSTRAINT [FK_EmployeeLoan];
GO
IF OBJECT_ID(N'[dbo].[FK_UserDashboardConfig]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DashboardConfigs] DROP CONSTRAINT [FK_UserDashboardConfig];
GO
IF OBJECT_ID(N'[dbo].[FK_OrganizationOrganizationSettings]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[OrganizationSettings] DROP CONSTRAINT [FK_OrganizationOrganizationSettings];
GO
IF OBJECT_ID(N'[dbo].[FK_OrganizationReport]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Reports] DROP CONSTRAINT [FK_OrganizationReport];
GO
IF OBJECT_ID(N'[dbo].[FK_ReportCustomReportSchedule]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CustomReportSchedules] DROP CONSTRAINT [FK_ReportCustomReportSchedule];
GO
IF OBJECT_ID(N'[dbo].[FK_ReportReportField]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ReportFields] DROP CONSTRAINT [FK_ReportReportField];
GO
IF OBJECT_ID(N'[dbo].[FK_FieldReportField]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ReportFields] DROP CONSTRAINT [FK_FieldReportField];
GO
IF OBJECT_ID(N'[dbo].[FK_DemographicDeposit]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Deposits] DROP CONSTRAINT [FK_DemographicDeposit];
GO
IF OBJECT_ID(N'[dbo].[FK_OrganizationSettingsFeedLog]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FeedLogs] DROP CONSTRAINT [FK_OrganizationSettingsFeedLog];
GO
IF OBJECT_ID(N'[dbo].[FK_RateTableOrganizationSettings]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RepaymentTables] DROP CONSTRAINT [FK_RateTableOrganizationSettings];
GO
IF OBJECT_ID(N'[dbo].[FK_LoanCharge]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Charges] DROP CONSTRAINT [FK_LoanCharge];
GO
IF OBJECT_ID(N'[dbo].[FK_DemographicLoan]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Loans] DROP CONSTRAINT [FK_DemographicLoan];
GO
IF OBJECT_ID(N'[dbo].[FK_AvailableEntityEntityOption]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AvailableEntityOptions] DROP CONSTRAINT [FK_AvailableEntityEntityOption];
GO
IF OBJECT_ID(N'[dbo].[FK_AvailableEntityReportEntity]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ReportEntities] DROP CONSTRAINT [FK_AvailableEntityReportEntity];
GO
IF OBJECT_ID(N'[dbo].[FK_ReportReportEntity]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ReportEntities] DROP CONSTRAINT [FK_ReportReportEntity];
GO
IF OBJECT_ID(N'[dbo].[FK_ReportEntityEntityOption]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EntityOptions] DROP CONSTRAINT [FK_ReportEntityEntityOption];
GO
IF OBJECT_ID(N'[dbo].[FK_AvailableEntityOptionEntityOption]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EntityOptions] DROP CONSTRAINT [FK_AvailableEntityOptionEntityOption];
GO
IF OBJECT_ID(N'[dbo].[FK_ReportEntityReportEntity]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ReportEntities] DROP CONSTRAINT [FK_ReportEntityReportEntity];
GO
IF OBJECT_ID(N'[dbo].[FK_ReportReportFieldChange]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ReportFieldChanges] DROP CONSTRAINT [FK_ReportReportFieldChange];
GO
IF OBJECT_ID(N'[dbo].[FK_FeedLogFeedLogDetail]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FeedLogDetails] DROP CONSTRAINT [FK_FeedLogFeedLogDetail];
GO
IF OBJECT_ID(N'[dbo].[FK_ReportReport834Options]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Report834Options] DROP CONSTRAINT [FK_ReportReport834Options];
GO
IF OBJECT_ID(N'[dbo].[FK_ReportTermination]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Terminations] DROP CONSTRAINT [FK_ReportTermination];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO
IF OBJECT_ID(N'[dbo].[Organizations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Organizations];
GO
IF OBJECT_ID(N'[dbo].[Roles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Roles];
GO
IF OBJECT_ID(N'[dbo].[Demographics]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Demographics];
GO
IF OBJECT_ID(N'[dbo].[Charges]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Charges];
GO
IF OBJECT_ID(N'[dbo].[Deposits]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Deposits];
GO
IF OBJECT_ID(N'[dbo].[DashboardConfigs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DashboardConfigs];
GO
IF OBJECT_ID(N'[dbo].[OrganizationSettings]', 'U') IS NOT NULL
    DROP TABLE [dbo].[OrganizationSettings];
GO
IF OBJECT_ID(N'[dbo].[Reports]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Reports];
GO
IF OBJECT_ID(N'[dbo].[CustomReportSchedules]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CustomReportSchedules];
GO
IF OBJECT_ID(N'[dbo].[ReportFields]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ReportFields];
GO
IF OBJECT_ID(N'[dbo].[Fields]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Fields];
GO
IF OBJECT_ID(N'[dbo].[FeedLogs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FeedLogs];
GO
IF OBJECT_ID(N'[dbo].[RepaymentTables]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RepaymentTables];
GO
IF OBJECT_ID(N'[dbo].[Loans]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Loans];
GO
IF OBJECT_ID(N'[dbo].[AvailableEntities]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AvailableEntities];
GO
IF OBJECT_ID(N'[dbo].[AvailableEntityOptions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AvailableEntityOptions];
GO
IF OBJECT_ID(N'[dbo].[ReportEntities]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ReportEntities];
GO
IF OBJECT_ID(N'[dbo].[EntityOptions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[EntityOptions];
GO
IF OBJECT_ID(N'[dbo].[ReportFieldChanges]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ReportFieldChanges];
GO
IF OBJECT_ID(N'[dbo].[FeedLogDetails]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FeedLogDetails];
GO
IF OBJECT_ID(N'[dbo].[Report834Options]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Report834Options];
GO
IF OBJECT_ID(N'[dbo].[CronLogs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CronLogs];
GO
IF OBJECT_ID(N'[dbo].[Terminations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Terminations];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [id] int IDENTITY(1,1) NOT NULL,
    [username] nvarchar(max)  NOT NULL,
    [email] nvarchar(max)  NOT NULL,
    [first_name] nvarchar(max)  NOT NULL,
    [last_name] nvarchar(max)  NOT NULL,
    [password] nvarchar(max)  NULL,
    [employee_id] int  NULL
);
GO

-- Creating table 'Organizations'
CREATE TABLE [dbo].[Organizations] (
    [id] int IDENTITY(1,1) NOT NULL,
    [name] nvarchar(max)  NOT NULL,
    [logo_url] nvarchar(max)  NULL,
    [color_1] nvarchar(max)  NULL,
    [color_2] nvarchar(max)  NULL,
    [company_id] nvarchar(max)  NULL,
    [payroll_id] nvarchar(max)  NULL,
    [parent_organization_id] int  NULL
);
GO

-- Creating table 'Roles'
CREATE TABLE [dbo].[Roles] (
    [id] int IDENTITY(1,1) NOT NULL,
    [access_level] int  NOT NULL,
    [role_type] nvarchar(max)  NOT NULL,
    [user_id] int  NOT NULL,
    [organization_id] int  NOT NULL
);
GO

-- Creating table 'Demographics'
CREATE TABLE [dbo].[Demographics] (
    [id] int IDENTITY(1,1) NOT NULL,
    [dob] datetime  NOT NULL,
    [social_security_num] nvarchar(max)  NOT NULL,
    [social_security_last_four] nvarchar(max)  NOT NULL,
    [address] nvarchar(max)  NOT NULL,
    [city] nvarchar(max)  NOT NULL,
    [state] nvarchar(max)  NOT NULL,
    [zip] nvarchar(max)  NOT NULL,
    [country] nvarchar(max)  NOT NULL,
    [company_identifier] nvarchar(max)  NOT NULL,
    [first_name] nvarchar(max)  NOT NULL,
    [last_name] nvarchar(max)  NOT NULL,
    [email] nvarchar(max)  NOT NULL,
    [balance] float  NOT NULL,
    [monthly_payment] float  NOT NULL,
    [terminated] bit  NOT NULL,
    [status] nvarchar(max)  NULL,
    [maximum_advance_amount] float  NOT NULL,
    [hsa_balance] float  NOT NULL,
    [payroll_identifier] nvarchar(max)  NOT NULL,
    [termination_date] datetime  NULL
);
GO

-- Creating table 'Charges'
CREATE TABLE [dbo].[Charges] (
    [id] int IDENTITY(1,1) NOT NULL,
    [transaction_date] datetime  NOT NULL,
    [claim_type] nvarchar(max)  NOT NULL,
    [description] nvarchar(max)  NOT NULL,
    [total_claim_amount] float  NOT NULL,
    [eligible_amount] float  NOT NULL,
    [approved_amount] float  NOT NULL,
    [ineligible_amount] float  NOT NULL,
    [pended_amount] float  NOT NULL,
    [denied_amount] float  NOT NULL,
    [denied_reason] nvarchar(max)  NOT NULL,
    [claim_number] nvarchar(max)  NOT NULL,
    [scc_mcc] nvarchar(max)  NOT NULL,
    [employee_id] int  NOT NULL,
    [loan_id] int  NULL
);
GO

-- Creating table 'Deposits'
CREATE TABLE [dbo].[Deposits] (
    [id] int IDENTITY(1,1) NOT NULL,
    [payroll_date] datetime  NOT NULL,
    [employee_contribution] float  NOT NULL,
    [employer_contribution] nvarchar(max)  NOT NULL,
    [loan_remaining_balance] float  NOT NULL,
    [plan_type] nvarchar(max)  NOT NULL,
    [plan_begin] nvarchar(max)  NOT NULL,
    [plan_end] nvarchar(max)  NOT NULL,
    [deposit_type] nvarchar(max)  NOT NULL,
    [employee_id] int  NOT NULL
);
GO

-- Creating table 'DashboardConfigs'
CREATE TABLE [dbo].[DashboardConfigs] (
    [id] int IDENTITY(1,1) NOT NULL,
    [quadrant_1] nvarchar(max)  NOT NULL,
    [quadrant_2] nvarchar(max)  NOT NULL,
    [quadrant_3] nvarchar(max)  NOT NULL,
    [quadrant_4] nvarchar(max)  NOT NULL,
    [user_id] int  NOT NULL
);
GO

-- Creating table 'OrganizationSettings'
CREATE TABLE [dbo].[OrganizationSettings] (
    [id] int IDENTITY(1,1) NOT NULL,
    [deposits_feed] nvarchar(max)  NOT NULL,
    [demographics_feed] nvarchar(max)  NOT NULL,
    [alegeus_feed] nvarchar(max)  NOT NULL,
    [withhold_entire_balance] bit  NOT NULL,
    [organization_id] int  NOT NULL
);
GO

-- Creating table 'Reports'
CREATE TABLE [dbo].[Reports] (
    [id] int IDENTITY(1,1) NOT NULL,
    [name] nvarchar(max)  NOT NULL,
    [grouping_field] nvarchar(max)  NOT NULL,
    [show_totals] bit  NOT NULL,
    [show_subtotals] bit  NOT NULL,
    [output_format] nvarchar(max)  NOT NULL,
    [delivery_type] nvarchar(max)  NOT NULL,
    [schedule_type] nvarchar(max)  NOT NULL,
    [delivery_email_address] nvarchar(max)  NOT NULL,
    [delivery_ftp_address] nvarchar(max)  NOT NULL,
    [delivery_ftp_username] nvarchar(max)  NOT NULL,
    [delivery_ftp_password] nvarchar(max)  NOT NULL,
    [delivery_ftp_port] nvarchar(max)  NOT NULL,
    [delivery_ftp_path] nvarchar(max)  NOT NULL,
    [is_report_builder] bit  NOT NULL,
    [global_report_orgs] nvarchar(max)  NULL,
    [header] nvarchar(max)  NULL,
    [footer] nvarchar(max)  NULL,
    [xml_file_path] nvarchar(max)  NULL,
    [output_filename] nvarchar(max)  NULL,
    [organization_id] int  NULL
);
GO

-- Creating table 'CustomReportSchedules'
CREATE TABLE [dbo].[CustomReportSchedules] (
    [id] int IDENTITY(1,1) NOT NULL,
    [month] int  NOT NULL,
    [day] int  NOT NULL,
    [recurrence] nvarchar(max)  NOT NULL,
    [start_date] nvarchar(max)  NOT NULL,
    [report_id] int  NOT NULL
);
GO

-- Creating table 'ReportFields'
CREATE TABLE [dbo].[ReportFields] (
    [id] int IDENTITY(1,1) NOT NULL,
    [name] nvarchar(max)  NOT NULL,
    [source] nvarchar(max)  NOT NULL,
    [format] nvarchar(max)  NOT NULL,
    [field_type] nvarchar(max)  NOT NULL,
    [calculation] nvarchar(max)  NOT NULL,
    [field_order] int  NOT NULL,
    [report_id] int  NOT NULL,
    [field_id] int  NOT NULL
);
GO

-- Creating table 'Fields'
CREATE TABLE [dbo].[Fields] (
    [id] int IDENTITY(1,1) NOT NULL,
    [source] nvarchar(max)  NOT NULL,
    [name] nvarchar(max)  NOT NULL,
    [field_key] nvarchar(max)  NOT NULL,
    [field_format] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'FeedLogs'
CREATE TABLE [dbo].[FeedLogs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [feed_timestamp] datetime  NOT NULL,
    [feed_name] nvarchar(max)  NOT NULL,
    [feed_direction] nvarchar(max)  NOT NULL,
    [feed_location] nvarchar(max)  NOT NULL,
    [successful] bit  NOT NULL,
    [organization_setting_id] int  NOT NULL
);
GO

-- Creating table 'RepaymentTables'
CREATE TABLE [dbo].[RepaymentTables] (
    [id] int IDENTITY(1,1) NOT NULL,
    [loan_amount_min] float  NOT NULL,
    [loan_amount_max] float  NOT NULL,
    [repaymant_amount] float  NOT NULL,
    [organization_settings_id] int  NOT NULL
);
GO

-- Creating table 'Loans'
CREATE TABLE [dbo].[Loans] (
    [id] int IDENTITY(1,1) NOT NULL,
    [loan_id] uniqueidentifier  NOT NULL,
    [balance] float  NOT NULL,
    [monthly_payment] float  NOT NULL,
    [last_payment_date] datetime  NOT NULL,
    [employee_id] int  NOT NULL
);
GO

-- Creating table 'AvailableEntities'
CREATE TABLE [dbo].[AvailableEntities] (
    [id] int IDENTITY(1,1) NOT NULL,
    [entity_type] nvarchar(max)  NOT NULL,
    [name] nvarchar(max)  NOT NULL,
    [valid_parent_type] nvarchar(max)  NULL
);
GO

-- Creating table 'AvailableEntityOptions'
CREATE TABLE [dbo].[AvailableEntityOptions] (
    [id] int IDENTITY(1,1) NOT NULL,
    [option_string] nvarchar(max)  NOT NULL,
    [available_entity_id] int  NOT NULL
);
GO

-- Creating table 'ReportEntities'
CREATE TABLE [dbo].[ReportEntities] (
    [id] int IDENTITY(1,1) NOT NULL,
    [indentation_level] int  NOT NULL,
    [caption] nvarchar(max)  NOT NULL,
    [format] nvarchar(max)  NOT NULL,
    [literal_value] nvarchar(max)  NULL,
    [option] nvarchar(max)  NULL,
    [section_delimiter] nvarchar(max)  NOT NULL,
    [format_json] nvarchar(max)  NULL,
    [map_json] nvarchar(max)  NULL,
    [format_option_fixed_width] int  NOT NULL,
    [format_option_fixed_width_val] int  NULL,
    [format_option_capitalize_entire_element] int  NOT NULL,
    [format_option_remove_characters] int  NOT NULL,
    [format_option_remove_characters_list] nvarchar(max)  NULL,
    [format_option_date] int  NOT NULL,
    [format_option_date_format] nvarchar(max)  NULL,
    [format_option_currency] int  NOT NULL,
    [format_option_currency_format] nvarchar(max)  NULL,
    [collapsed] bit  NOT NULL,
    [available_entity_id] int  NOT NULL,
    [report_id] int  NOT NULL,
    [parent_entities_id] int  NULL
);
GO

-- Creating table 'EntityOptions'
CREATE TABLE [dbo].[EntityOptions] (
    [id] int IDENTITY(1,1) NOT NULL,
    [option_value] nvarchar(max)  NOT NULL,
    [report_entity_id] int  NOT NULL,
    [available_entity_option_id] int  NOT NULL
);
GO

-- Creating table 'ReportFieldChanges'
CREATE TABLE [dbo].[ReportFieldChanges] (
    [id] int IDENTITY(1,1) NOT NULL,
    [field_name] nvarchar(max)  NOT NULL,
    [field_value] nvarchar(max)  NOT NULL,
    [unique_identifier] nvarchar(max)  NOT NULL,
    [record_type] nvarchar(max)  NOT NULL,
    [report_id] int  NOT NULL
);
GO

-- Creating table 'FeedLogDetails'
CREATE TABLE [dbo].[FeedLogDetails] (
    [id] int IDENTITY(1,1) NOT NULL,
    [log_message] nvarchar(max)  NOT NULL,
    [message_timestamp] datetime  NOT NULL,
    [feed_log_Id] int  NOT NULL
);
GO

-- Creating table 'Report834Options'
CREATE TABLE [dbo].[Report834Options] (
    [id] int IDENTITY(1,1) NOT NULL,
    [isa02] nvarchar(max)  NOT NULL,
    [isa03] nvarchar(max)  NOT NULL,
    [isa04] nvarchar(max)  NOT NULL,
    [isa05] nvarchar(max)  NOT NULL,
    [isa06] nvarchar(max)  NOT NULL,
    [isa07] nvarchar(max)  NOT NULL,
    [isa08] nvarchar(max)  NOT NULL,
    [isa13] nvarchar(max)  NOT NULL,
    [isa14] nvarchar(max)  NOT NULL,
    [gs02] nvarchar(max)  NOT NULL,
    [gs03] nvarchar(max)  NOT NULL,
    [gs06] nvarchar(max)  NOT NULL,
    [gs08] nvarchar(max)  NOT NULL,
    [bgn02] nvarchar(max)  NOT NULL,
    [bgn08] nvarchar(max)  NOT NULL,
    [st02] nvarchar(max)  NOT NULL,
    [dtp03] nvarchar(max)  NOT NULL,
    [isa01] nvarchar(max)  NOT NULL,
    [isa15] nvarchar(max)  NOT NULL,
    [bgn06] nvarchar(max)  NOT NULL,
    [ref02] nvarchar(max)  NOT NULL,
    [dtp01] nvarchar(max)  NOT NULL,
    [n102a] nvarchar(max)  NOT NULL,
    [n103a] nvarchar(max)  NOT NULL,
    [n104a] nvarchar(max)  NOT NULL,
    [n102b] nvarchar(max)  NOT NULL,
    [n103b] nvarchar(max)  NOT NULL,
    [n104b] nvarchar(max)  NOT NULL,
    [n102c] nvarchar(max)  NOT NULL,
    [n103c] nvarchar(max)  NOT NULL,
    [n104c] nvarchar(max)  NOT NULL,
    [dtp01b] nvarchar(max)  NOT NULL,
    [dtp03b] nvarchar(max)  NOT NULL,
    [include_qty_a] int  NOT NULL,
    [include_qty_b] int  NOT NULL,
    [include_qty_c] int  NOT NULL,
    [include_n1_a] int  NOT NULL,
    [include_n1_b] int  NOT NULL,
    [include_n1_c] int  NOT NULL,
    [include_dtp_a] int  NOT NULL,
    [include_dtp_b] int  NOT NULL,
    [remove_line_breaks] bit  NOT NULL,
    [remove_tildes] bit  NOT NULL,
    [report_id] int  NOT NULL
);
GO

-- Creating table 'CronLogs'
CREATE TABLE [dbo].[CronLogs] (
    [id] int IDENTITY(1,1) NOT NULL,
    [cron_timestamp] datetime  NOT NULL,
    [cron_type] nvarchar(max)  NOT NULL,
    [cron_log] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Terminations'
CREATE TABLE [dbo].[Terminations] (
    [id] int IDENTITY(1,1) NOT NULL,
    [employee_key] nvarchar(max)  NOT NULL,
    [report_id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [id] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Organizations'
ALTER TABLE [dbo].[Organizations]
ADD CONSTRAINT [PK_Organizations]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Roles'
ALTER TABLE [dbo].[Roles]
ADD CONSTRAINT [PK_Roles]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Demographics'
ALTER TABLE [dbo].[Demographics]
ADD CONSTRAINT [PK_Demographics]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Charges'
ALTER TABLE [dbo].[Charges]
ADD CONSTRAINT [PK_Charges]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Deposits'
ALTER TABLE [dbo].[Deposits]
ADD CONSTRAINT [PK_Deposits]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'DashboardConfigs'
ALTER TABLE [dbo].[DashboardConfigs]
ADD CONSTRAINT [PK_DashboardConfigs]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'OrganizationSettings'
ALTER TABLE [dbo].[OrganizationSettings]
ADD CONSTRAINT [PK_OrganizationSettings]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Reports'
ALTER TABLE [dbo].[Reports]
ADD CONSTRAINT [PK_Reports]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'CustomReportSchedules'
ALTER TABLE [dbo].[CustomReportSchedules]
ADD CONSTRAINT [PK_CustomReportSchedules]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'ReportFields'
ALTER TABLE [dbo].[ReportFields]
ADD CONSTRAINT [PK_ReportFields]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Fields'
ALTER TABLE [dbo].[Fields]
ADD CONSTRAINT [PK_Fields]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [Id] in table 'FeedLogs'
ALTER TABLE [dbo].[FeedLogs]
ADD CONSTRAINT [PK_FeedLogs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [id] in table 'RepaymentTables'
ALTER TABLE [dbo].[RepaymentTables]
ADD CONSTRAINT [PK_RepaymentTables]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Loans'
ALTER TABLE [dbo].[Loans]
ADD CONSTRAINT [PK_Loans]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'AvailableEntities'
ALTER TABLE [dbo].[AvailableEntities]
ADD CONSTRAINT [PK_AvailableEntities]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'AvailableEntityOptions'
ALTER TABLE [dbo].[AvailableEntityOptions]
ADD CONSTRAINT [PK_AvailableEntityOptions]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'ReportEntities'
ALTER TABLE [dbo].[ReportEntities]
ADD CONSTRAINT [PK_ReportEntities]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'EntityOptions'
ALTER TABLE [dbo].[EntityOptions]
ADD CONSTRAINT [PK_EntityOptions]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'ReportFieldChanges'
ALTER TABLE [dbo].[ReportFieldChanges]
ADD CONSTRAINT [PK_ReportFieldChanges]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'FeedLogDetails'
ALTER TABLE [dbo].[FeedLogDetails]
ADD CONSTRAINT [PK_FeedLogDetails]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Report834Options'
ALTER TABLE [dbo].[Report834Options]
ADD CONSTRAINT [PK_Report834Options]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'CronLogs'
ALTER TABLE [dbo].[CronLogs]
ADD CONSTRAINT [PK_CronLogs]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Terminations'
ALTER TABLE [dbo].[Terminations]
ADD CONSTRAINT [PK_Terminations]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [user_id] in table 'Roles'
ALTER TABLE [dbo].[Roles]
ADD CONSTRAINT [FK_UserRole]
    FOREIGN KEY ([user_id])
    REFERENCES [dbo].[Users]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserRole'
CREATE INDEX [IX_FK_UserRole]
ON [dbo].[Roles]
    ([user_id]);
GO

-- Creating foreign key on [organization_id] in table 'Roles'
ALTER TABLE [dbo].[Roles]
ADD CONSTRAINT [FK_OrganizationRole]
    FOREIGN KEY ([organization_id])
    REFERENCES [dbo].[Organizations]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_OrganizationRole'
CREATE INDEX [IX_FK_OrganizationRole]
ON [dbo].[Roles]
    ([organization_id]);
GO

-- Creating foreign key on [parent_organization_id] in table 'Organizations'
ALTER TABLE [dbo].[Organizations]
ADD CONSTRAINT [FK_OrganizationOrganization]
    FOREIGN KEY ([parent_organization_id])
    REFERENCES [dbo].[Organizations]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_OrganizationOrganization'
CREATE INDEX [IX_FK_OrganizationOrganization]
ON [dbo].[Organizations]
    ([parent_organization_id]);
GO

-- Creating foreign key on [employee_id] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [FK_EmployeeUser]
    FOREIGN KEY ([employee_id])
    REFERENCES [dbo].[Demographics]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_EmployeeUser'
CREATE INDEX [IX_FK_EmployeeUser]
ON [dbo].[Users]
    ([employee_id]);
GO

-- Creating foreign key on [employee_id] in table 'Charges'
ALTER TABLE [dbo].[Charges]
ADD CONSTRAINT [FK_EmployeeLoan]
    FOREIGN KEY ([employee_id])
    REFERENCES [dbo].[Demographics]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_EmployeeLoan'
CREATE INDEX [IX_FK_EmployeeLoan]
ON [dbo].[Charges]
    ([employee_id]);
GO

-- Creating foreign key on [user_id] in table 'DashboardConfigs'
ALTER TABLE [dbo].[DashboardConfigs]
ADD CONSTRAINT [FK_UserDashboardConfig]
    FOREIGN KEY ([user_id])
    REFERENCES [dbo].[Users]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserDashboardConfig'
CREATE INDEX [IX_FK_UserDashboardConfig]
ON [dbo].[DashboardConfigs]
    ([user_id]);
GO

-- Creating foreign key on [organization_id] in table 'OrganizationSettings'
ALTER TABLE [dbo].[OrganizationSettings]
ADD CONSTRAINT [FK_OrganizationOrganizationSettings]
    FOREIGN KEY ([organization_id])
    REFERENCES [dbo].[Organizations]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_OrganizationOrganizationSettings'
CREATE INDEX [IX_FK_OrganizationOrganizationSettings]
ON [dbo].[OrganizationSettings]
    ([organization_id]);
GO

-- Creating foreign key on [organization_id] in table 'Reports'
ALTER TABLE [dbo].[Reports]
ADD CONSTRAINT [FK_OrganizationReport]
    FOREIGN KEY ([organization_id])
    REFERENCES [dbo].[Organizations]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_OrganizationReport'
CREATE INDEX [IX_FK_OrganizationReport]
ON [dbo].[Reports]
    ([organization_id]);
GO

-- Creating foreign key on [report_id] in table 'CustomReportSchedules'
ALTER TABLE [dbo].[CustomReportSchedules]
ADD CONSTRAINT [FK_ReportCustomReportSchedule]
    FOREIGN KEY ([report_id])
    REFERENCES [dbo].[Reports]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ReportCustomReportSchedule'
CREATE INDEX [IX_FK_ReportCustomReportSchedule]
ON [dbo].[CustomReportSchedules]
    ([report_id]);
GO

-- Creating foreign key on [report_id] in table 'ReportFields'
ALTER TABLE [dbo].[ReportFields]
ADD CONSTRAINT [FK_ReportReportField]
    FOREIGN KEY ([report_id])
    REFERENCES [dbo].[Reports]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ReportReportField'
CREATE INDEX [IX_FK_ReportReportField]
ON [dbo].[ReportFields]
    ([report_id]);
GO

-- Creating foreign key on [field_id] in table 'ReportFields'
ALTER TABLE [dbo].[ReportFields]
ADD CONSTRAINT [FK_FieldReportField]
    FOREIGN KEY ([field_id])
    REFERENCES [dbo].[Fields]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FieldReportField'
CREATE INDEX [IX_FK_FieldReportField]
ON [dbo].[ReportFields]
    ([field_id]);
GO

-- Creating foreign key on [employee_id] in table 'Deposits'
ALTER TABLE [dbo].[Deposits]
ADD CONSTRAINT [FK_DemographicDeposit]
    FOREIGN KEY ([employee_id])
    REFERENCES [dbo].[Demographics]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_DemographicDeposit'
CREATE INDEX [IX_FK_DemographicDeposit]
ON [dbo].[Deposits]
    ([employee_id]);
GO

-- Creating foreign key on [organization_setting_id] in table 'FeedLogs'
ALTER TABLE [dbo].[FeedLogs]
ADD CONSTRAINT [FK_OrganizationSettingsFeedLog]
    FOREIGN KEY ([organization_setting_id])
    REFERENCES [dbo].[OrganizationSettings]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_OrganizationSettingsFeedLog'
CREATE INDEX [IX_FK_OrganizationSettingsFeedLog]
ON [dbo].[FeedLogs]
    ([organization_setting_id]);
GO

-- Creating foreign key on [organization_settings_id] in table 'RepaymentTables'
ALTER TABLE [dbo].[RepaymentTables]
ADD CONSTRAINT [FK_RateTableOrganizationSettings]
    FOREIGN KEY ([organization_settings_id])
    REFERENCES [dbo].[OrganizationSettings]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_RateTableOrganizationSettings'
CREATE INDEX [IX_FK_RateTableOrganizationSettings]
ON [dbo].[RepaymentTables]
    ([organization_settings_id]);
GO

-- Creating foreign key on [loan_id] in table 'Charges'
ALTER TABLE [dbo].[Charges]
ADD CONSTRAINT [FK_LoanCharge]
    FOREIGN KEY ([loan_id])
    REFERENCES [dbo].[Loans]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_LoanCharge'
CREATE INDEX [IX_FK_LoanCharge]
ON [dbo].[Charges]
    ([loan_id]);
GO

-- Creating foreign key on [employee_id] in table 'Loans'
ALTER TABLE [dbo].[Loans]
ADD CONSTRAINT [FK_DemographicLoan]
    FOREIGN KEY ([employee_id])
    REFERENCES [dbo].[Demographics]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_DemographicLoan'
CREATE INDEX [IX_FK_DemographicLoan]
ON [dbo].[Loans]
    ([employee_id]);
GO

-- Creating foreign key on [available_entity_id] in table 'AvailableEntityOptions'
ALTER TABLE [dbo].[AvailableEntityOptions]
ADD CONSTRAINT [FK_AvailableEntityEntityOption]
    FOREIGN KEY ([available_entity_id])
    REFERENCES [dbo].[AvailableEntities]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AvailableEntityEntityOption'
CREATE INDEX [IX_FK_AvailableEntityEntityOption]
ON [dbo].[AvailableEntityOptions]
    ([available_entity_id]);
GO

-- Creating foreign key on [available_entity_id] in table 'ReportEntities'
ALTER TABLE [dbo].[ReportEntities]
ADD CONSTRAINT [FK_AvailableEntityReportEntity]
    FOREIGN KEY ([available_entity_id])
    REFERENCES [dbo].[AvailableEntities]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AvailableEntityReportEntity'
CREATE INDEX [IX_FK_AvailableEntityReportEntity]
ON [dbo].[ReportEntities]
    ([available_entity_id]);
GO

-- Creating foreign key on [report_id] in table 'ReportEntities'
ALTER TABLE [dbo].[ReportEntities]
ADD CONSTRAINT [FK_ReportReportEntity]
    FOREIGN KEY ([report_id])
    REFERENCES [dbo].[Reports]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ReportReportEntity'
CREATE INDEX [IX_FK_ReportReportEntity]
ON [dbo].[ReportEntities]
    ([report_id]);
GO

-- Creating foreign key on [report_entity_id] in table 'EntityOptions'
ALTER TABLE [dbo].[EntityOptions]
ADD CONSTRAINT [FK_ReportEntityEntityOption]
    FOREIGN KEY ([report_entity_id])
    REFERENCES [dbo].[ReportEntities]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ReportEntityEntityOption'
CREATE INDEX [IX_FK_ReportEntityEntityOption]
ON [dbo].[EntityOptions]
    ([report_entity_id]);
GO

-- Creating foreign key on [available_entity_option_id] in table 'EntityOptions'
ALTER TABLE [dbo].[EntityOptions]
ADD CONSTRAINT [FK_AvailableEntityOptionEntityOption]
    FOREIGN KEY ([available_entity_option_id])
    REFERENCES [dbo].[AvailableEntityOptions]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AvailableEntityOptionEntityOption'
CREATE INDEX [IX_FK_AvailableEntityOptionEntityOption]
ON [dbo].[EntityOptions]
    ([available_entity_option_id]);
GO

-- Creating foreign key on [parent_entities_id] in table 'ReportEntities'
ALTER TABLE [dbo].[ReportEntities]
ADD CONSTRAINT [FK_ReportEntityReportEntity]
    FOREIGN KEY ([parent_entities_id])
    REFERENCES [dbo].[ReportEntities]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ReportEntityReportEntity'
CREATE INDEX [IX_FK_ReportEntityReportEntity]
ON [dbo].[ReportEntities]
    ([parent_entities_id]);
GO

-- Creating foreign key on [report_id] in table 'ReportFieldChanges'
ALTER TABLE [dbo].[ReportFieldChanges]
ADD CONSTRAINT [FK_ReportReportFieldChange]
    FOREIGN KEY ([report_id])
    REFERENCES [dbo].[Reports]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ReportReportFieldChange'
CREATE INDEX [IX_FK_ReportReportFieldChange]
ON [dbo].[ReportFieldChanges]
    ([report_id]);
GO

-- Creating foreign key on [feed_log_Id] in table 'FeedLogDetails'
ALTER TABLE [dbo].[FeedLogDetails]
ADD CONSTRAINT [FK_FeedLogFeedLogDetail]
    FOREIGN KEY ([feed_log_Id])
    REFERENCES [dbo].[FeedLogs]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FeedLogFeedLogDetail'
CREATE INDEX [IX_FK_FeedLogFeedLogDetail]
ON [dbo].[FeedLogDetails]
    ([feed_log_Id]);
GO

-- Creating foreign key on [report_id] in table 'Report834Options'
ALTER TABLE [dbo].[Report834Options]
ADD CONSTRAINT [FK_ReportReport834Options]
    FOREIGN KEY ([report_id])
    REFERENCES [dbo].[Reports]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ReportReport834Options'
CREATE INDEX [IX_FK_ReportReport834Options]
ON [dbo].[Report834Options]
    ([report_id]);
GO

-- Creating foreign key on [report_id] in table 'Terminations'
ALTER TABLE [dbo].[Terminations]
ADD CONSTRAINT [FK_ReportTermination]
    FOREIGN KEY ([report_id])
    REFERENCES [dbo].[Reports]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ReportTermination'
CREATE INDEX [IX_FK_ReportTermination]
ON [dbo].[Terminations]
    ([report_id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------