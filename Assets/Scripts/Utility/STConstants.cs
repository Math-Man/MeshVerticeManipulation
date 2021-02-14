using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Scripting;

public static class STConstants
{
    public enum Tags
    {
        [Description("Structure")]
        STRUCTURE

    }


    public enum STType
    {
        [Description("Destroy")]
        DESTROY = -2,
        [Description("None")]
        NONE = -1,
        [Description("Core")]
        CORE = 0,
        [Description("Relay")]
        RELAY = 1,
        [Description("Generator")]
        GENERATOR = 2,
        [Description("Miner")]
        MINER = 3,
        [Description("Basic Turret")]
        TURRET_BASIC = 4,
        [Description("Missile Turret")]
        TURRET_MISSILE = 5,
        [Description("Beam Turret")]
        TURRET_BEAM = 6,
        [Description("Flak Turret")]
        TURRET_FLAK = 7,
        
    }


}




public static class DescriptorExtension
{
    public static string GetDescriptionString(this STConstants.Tags val)
    {
        DescriptionAttribute[] attributes = (DescriptionAttribute[])val
           .GetType()
           .GetField(val.ToString())
           .GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : string.Empty;
    }

    public static string GetDescriptionString(this STConstants.STType val)
    {
        DescriptionAttribute[] attributes = (DescriptionAttribute[])val
           .GetType()
           .GetField(val.ToString())
           .GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : string.Empty;
    }

}