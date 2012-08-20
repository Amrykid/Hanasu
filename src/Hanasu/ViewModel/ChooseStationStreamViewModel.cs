using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crystal.Core;
using Hanasu.Core.Preprocessor;
using System.Collections.ObjectModel;
using Crystal.Command;
using Crystal.Messaging;

namespace Hanasu.ViewModel
{
    public class ChooseStationStreamViewModel: BaseViewModel
    {
        public ChooseStationStreamViewModel()
        {

            AcceptStreamCommand = this.CommandManager.CreateCommandFromBinding("SelectedStream",
                (s, e) => SelectedStream != null,
                (o) =>
                {
                    Messenger.PushMessage(this, "StationStreamChoosen", SelectedStream);

                    ViewModelOperations.CloseWindow(this, true);
                });

            CancelCommand = new CrystalCommand(this, true, (o) =>
                {
                    Messenger.PushMessage(this, "StationStreamChoosen", null);

                    ViewModelOperations.CloseWindow(this, false);
                });
        }

        [MessageHandler("StationStreamWindowStreamsPushed")]
        public void HandleStreams(object data)
        {
            AvailableStreams = (dynamic)data;
        }

        public CrystalCommand AcceptStreamCommand { get; set; }
        public CrystalCommand CancelCommand { get; set; }

        public IList<IMultiStreamEntry> AvailableStreams
        {
            get { return (dynamic)this.GetProperty("AvailableStreams"); }
            set { this.SetProperty("AvailableStreams", value); }
        }

        public IMultiStreamEntry SelectedStream
        {
            get { return (IMultiStreamEntry)this.GetProperty("SelectedStream"); }
            set { this.SetProperty("SelectedStream", value); }
        }
    }
}
