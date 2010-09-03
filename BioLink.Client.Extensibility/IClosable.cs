﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Extensibility {

    public interface IClosable : IDisposable {

        bool RequestClose();

    }
}
