/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ivrToolkit.Core
{
/// <summary>
/// The state of the device line.
/// </summary>
    public enum LineStatusTypes
    {
        /// <summary>
        /// Closed and no longer available. It has been recycled.
        /// </summary>
        Closed,
        /// <summary>
        /// Connected to a remote caller.
        /// </summary>
        Connected,
        /// <summary>
        /// Line handset is set down.
        /// </summary>
        OnHook,
        /// <summary>
        /// Listening for an incomming call.
        /// </summary>
        AcceptingCalls,
        /// <summary>
        /// Line handset has been picked up.
        /// </summary>
        OffHook
    }
}
