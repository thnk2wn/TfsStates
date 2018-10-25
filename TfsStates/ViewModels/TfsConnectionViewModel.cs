using System;
using System.Collections.Generic;

namespace TfsStates.ViewModels
{
    public class TfsConnectionViewModel
    {
        public TfsConnectionViewModel()
        {
            Connections = new List<TfsConnectionItemViewModel>();
        }

        public List<TfsConnectionItemViewModel> Connections { get; set; }

        public Guid? ActiveConnectionId { get; set; }
    }
}
