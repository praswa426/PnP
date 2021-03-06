﻿using Microsoft.SharePoint.Client;
using OfficeDevPnP.PowerShell.CmdletHelpAttributes;
using OfficeDevPnP.PowerShell.Commands.Base;
using OfficeDevPnP.PowerShell.Commands.Enums;
using System.Management.Automation;

namespace OfficeDevPnP.PowerShell.Commands.Principals
{
    [Cmdlet("New", "SPOGroup")]
    [CmdletHelp("Adds a user to the build-in Site User Info List and returns a user object")]
    [CmdletExample(Code = @"
PS:> New-SPOUser -LogonName user@company.com
")]
    public class NewGroup : SPOWebCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Title = string.Empty;

        [Parameter(Mandatory = false)]
        public string Description;

        [Parameter(Mandatory = false)]
        public string Owner;

        [Parameter(Mandatory = false)]
        public SwitchParameter AllowRequestToJoinLeave;

        [Parameter(Mandatory = false)]
        public SwitchParameter AutoAcceptRequestToJoinLeave;

        [Parameter(Mandatory = false, DontShow=true)] // Not promoted to use anymore. Use Set-SPOGroup
        public AssociatedGroupTypeEnum SetAssociatedGroup = AssociatedGroupTypeEnum.None;

        protected override void ExecuteCmdlet()
        {
            var web = this.SelectedWeb;

            GroupCreationInformation groupCI = new GroupCreationInformation();
            groupCI.Title = Title;
            groupCI.Description = Description;

            var group = web.SiteGroups.Add(groupCI);

            ClientContext.Load(group);
            ClientContext.Load(group.Users);
            ClientContext.ExecuteQuery();
            bool dirty = false;
            if (AllowRequestToJoinLeave)
            {
                group.AllowRequestToJoinLeave = true;
                dirty = true;
            }

            if (AutoAcceptRequestToJoinLeave)
            {
                group.AutoAcceptRequestToJoinLeave = true;
                dirty = true;
            }
            if (dirty)
            {
                group.Update();
                ClientContext.ExecuteQuery();
            }


            if (!string.IsNullOrEmpty(Owner))
            {
                Principal groupOwner = null;

                try
                {
                    groupOwner = web.EnsureUser(Owner);
                    group.Owner = groupOwner;
                    group.Update();
                    ClientContext.ExecuteQuery();
                }
                catch
                {
                    groupOwner = web.SiteGroups.GetByName(Owner);
                    group.Owner = groupOwner;
                    group.Update();
                    ClientContext.ExecuteQuery();
                }
            }


            if (SetAssociatedGroup != AssociatedGroupTypeEnum.None)
            {
                switch (SetAssociatedGroup)
                {
                    case AssociatedGroupTypeEnum.Visitors:
                        {
                            web.AssociateDefaultGroups(null, null, group);
                            break;
                        }
                    case AssociatedGroupTypeEnum.Members:
                        {
                            web.AssociateDefaultGroups(null, group, null);
                            break;
                        }
                    case AssociatedGroupTypeEnum.Owners:
                        {
                            web.AssociateDefaultGroups(group, null, null);
                            break;
                        }
                }
            }
            ClientContext.ExecuteQuery();
            WriteObject(group);
        }
    }
}
