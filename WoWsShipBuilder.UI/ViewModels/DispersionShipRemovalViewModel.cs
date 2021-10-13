using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Metadata;
using ReactiveUI;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class DispersionShipRemovalViewModel : ViewModelBase
    {
        public DispersionShipRemovalViewModel(List<string> currentShipList)
        {
            CurrentShipList = new AvaloniaList<string>(currentShipList);
        }

        private AvaloniaList<string> currentShipList = new();

        public AvaloniaList<string> CurrentShipList
        {
            get => currentShipList;
            set => this.RaiseAndSetIfChanged(ref currentShipList, value);
        }

        private AvaloniaList<string> shipsToDeleteList = new();

        public AvaloniaList<string> ShipsToDeleteList
        {
            get => shipsToDeleteList;
            set => this.RaiseAndSetIfChanged(ref shipsToDeleteList, value);
        }

        private List<string> currentSelection = new();

        public List<string> CurrentSelection
        {
            get => currentSelection;
            set => this.RaiseAndSetIfChanged(ref currentSelection, value);
        }

        private List<string> removeSelection = new();

        public List<string> RemoveSelection
        {
            get => removeSelection;
            set => this.RaiseAndSetIfChanged(ref removeSelection, value);
        }

        public void RemoveShips()
        {
            var curr = CurrentSelection.ToList();
            ShipsToDeleteList.AddRange(curr);
            CurrentShipList.RemoveAll(curr);
        }

        [DependsOn(nameof(CurrentSelection))]
        public bool CanRemoveShips()
        {
            if (CurrentSelection.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RestoreShips()
        {
            var rem = RemoveSelection.ToList();
            CurrentShipList.AddRange(rem);
            ShipsToDeleteList.RemoveAll(rem);
        }

        [DependsOn(nameof(RemoveSelection))]
        public bool CanRestoreShips()
        {
            if (RemoveSelection.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
