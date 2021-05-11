using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EDITgui
{
    public class manageTumorGroups
    {
        Context context;
        TumorGroupsWindow tumorGroupsWindow;
        tumorCheckbox cb;

        public manageTumorGroups(Context context)
        {
            this.context = context;
        }

        public void CreateTumorGroupsWindow(tumorCheckbox cb) 
        {
            this.cb = cb;

            if (tumorGroupsWindow == null)
            {
                this.tumorGroupsWindow = new TumorGroupsWindow(this);
                foreach (string tumorName in context.getImages().getTumorGroups())
                {
                    TumorGroupItem tumorGroupItem = new TumorGroupItem(this.tumorGroupsWindow, tumorName);
                    tumorGroupsWindow.groupItems.Children.Add(tumorGroupItem);
                }
            }
        }

        public string checkIfGroupAlreadyExists(string group)
        {
            if (context.getImages().getTumorGroups().Contains(group))
            {
                return context.getMessages().thisTumorgroupAlreadyExists;
            }
            return null;
        }


        public void addGroup(string groupName)
        {
            context.getImages().addTumorGroup(groupName);
        }

        public void removeGroup(string groupName)
        {
            context.getImages().removeTumorGroup(groupName);
        }

        public void onTumorGrousWindowClosing()
        {
            this.tumorGroupsWindow = null;
            cb.groupOptionsDropdown.SelectedItem = null;
            cb.updateAfterCreatingNewGroup();
        }

    }
}
