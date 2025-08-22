namespace MauiSender.Models
{
    public class SelectorMap
    {
        public string LoginInput { get; set; } = "input[name='login'], #login";
        public string PasswordInput { get; set; } = "input[name='password'], #password";
        public string SubmitButton { get; set; } = "button[type='submit'], .submit-btn";
        public string ChatInput { get; set; } = "textarea[name='message'], #messageBox";
        public string SendButton { get; set; } = "button.send, .send-btn";
        public string ListItem { get; set; } = ".user-list-item, .contact";
    }
}
