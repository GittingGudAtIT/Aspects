﻿using System;

namespace Aspects.Interfaces
{
    public interface IAutoToStringAttribute 
    {
        DataMemberKind DataMemberKind { get; }

        Accessibility Accessibility { get; }
    }

    public interface IToStringAttribute { }

    public interface IToStringExcludeAttribute { }
}
