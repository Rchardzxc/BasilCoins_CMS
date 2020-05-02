using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetFwTypeLib;
using z.Data;

namespace InSys.Worker.Library.Classes
{
    public class FirewallException
    {
        public static void AddFirewallException(string port, string applicationName)
        {
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            INetFwRule2 firewallrule = null;

            firewallrule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));

            // INBOUND RULE
            if (firewallPolicy.Rules.OfType<INetFwRule>().Where(x => x.Name == applicationName && x.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN).Count() == 0)
            {
                firewallrule.Name = applicationName;
                firewallrule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                firewallrule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                firewallrule.Enabled = true;
                firewallrule.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP.ToInt32(); //NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
                firewallrule.LocalPorts = port;
                firewallrule.InterfaceTypes = "All";

                firewallPolicy.Rules.Add(firewallrule);
            }
            else
            {
                firewallrule = firewallPolicy.Rules.OfType<INetFwRule>().Where(x => x.Name == applicationName && x.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN).FirstOrDefault() == null ? null : firewallPolicy.Rules.OfType<INetFwRule>().Where(x => x.Name == applicationName && x.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN).First() as INetFwRule2;
                firewallrule.LocalPorts = port;
            }

            // OUTBOUND RULE
            if (firewallPolicy.Rules.OfType<INetFwRule>().Where(x => x.Name == applicationName && x.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT).Count() == 0)
            {
                firewallrule = null/* TODO Change to default(_) if this is not a reference type */;
                firewallrule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                firewallrule.Name = applicationName;
                firewallrule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                firewallrule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                firewallrule.Enabled = true;
                firewallrule.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP.ToInt32();
                firewallrule.LocalPorts = port;
                firewallrule.InterfaceTypes = "All";
                firewallPolicy.Rules.Add(firewallrule);
            }
            else
            {
                firewallrule = firewallPolicy.Rules.OfType<INetFwRule>().Where(x => x.Name == applicationName & x.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT).FirstOrDefault() == null ? null : firewallPolicy.Rules.OfType<INetFwRule>().Where(x => x.Name == applicationName & x.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT).FirstOrDefault() as INetFwRule2;
                firewallrule.LocalPorts = port;
            }
        }

    }
}
