using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiSender.Models;
using MauiSender.Services;

namespace MauiSender.ViewModels
{
    public class SenderViewModel : INotifyPropertyChanged
    {
        private readonly AccountService _accountService;
        private readonly CampaignRunner _runner;

        private string _selectedTemplate;
        public string SelectedTemplate
        {
            get => _selectedTemplate;
            set
            {
                _selectedTemplate = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Account> Accounts { get; set; }
        public ObservableCollection<string> Templates { get; set; }

        public ICommand StartCommand { get; }

        public SenderViewModel(AccountService accountService, CampaignRunner runner)
        {
            _accountService = accountService;
            _runner = runner;

            Accounts = new ObservableCollection<Account>(_accountService.LoadAccounts());
            Templates = new ObservableCollection<string> { "Привіт!", "Доброго дня!", "Ми раді вам!" };

            StartCommand = new Command(async () =>
            {
                if (!string.IsNullOrEmpty(SelectedTemplate))
                {
                    await _runner.StartCampaignAsync(Accounts.ToList(), SelectedTemplate);
                }
            });
        }

        public void AttachDom(DomBridge dom)
        {
            _runner.AttachDom(dom);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
