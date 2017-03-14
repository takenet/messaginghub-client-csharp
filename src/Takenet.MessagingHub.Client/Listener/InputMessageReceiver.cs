using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client.Host;
using Lime.Messaging.Contents;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Takenet.MessagingHub.Client.Extensions.Session;
using System.Text.RegularExpressions;
using Lime.Protocol.Serialization;
using System.Globalization;

namespace Takenet.MessagingHub.Client.Listener
{
    public class InputMessageReceiver : IMessageReceiver
    {
        private const string INPUT_VALIDATION_KEY = "InputValidation";

        private readonly IMessagingHubSender _sender;
        private readonly ISessionManager _sessionManager;
        private readonly InputSettings _settings;
        private readonly IDocumentSerializer _documentSerializer;
        
        public InputMessageReceiver(IMessagingHubSender sender, ISessionManager sessionManager, IDictionary<string, object> settings)
        {
            _sender = sender;
            _sessionManager = sessionManager;
            _settings = InputSettings.Parse(settings);
            _settings.Validate();
            _documentSerializer = new DocumentSerializer();
        }

        public async Task ReceiveAsync(Message envelope, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!await ValidateInputAsync(envelope, cancellationToken)) return;

            // Save the value
            var variableValue = _documentSerializer.Serialize(envelope.Content);
            await _sessionManager.AddVariableAsync(envelope.From, _settings.VariableName, variableValue, cancellationToken);

            // Configure for the next receiver
            if (_settings.Validation != null)
            {
                var validationJson = JsonConvert.SerializeObject(_settings.Validation, Application.SerializerSettings);
                await _sessionManager.AddVariableAsync(envelope.From, INPUT_VALIDATION_KEY, validationJson, cancellationToken);
            }

            // Send the label
            await _sender.SendMessageAsync(_settings.Label.ToDocument(), envelope.From, cancellationToken);
        }

        public async Task<bool> ValidateInputAsync(Message envelope, CancellationToken cancellationToken)
        {
            var validationJson = await _sessionManager.GetVariableAsync(envelope.From, INPUT_VALIDATION_KEY, cancellationToken);
            if (validationJson == null) return true;

            var inputValidation = JsonConvert.DeserializeObject<InputValidation>(validationJson, Application.SerializerSettings);
            if (ValidateRule(envelope.Content, inputValidation)) return true;

            await _sender.SendMessageAsync(inputValidation.Error ?? "An validation error has occurred", envelope.From, cancellationToken);
            return false;
        }

        private static bool ValidateRule(Document content, InputValidation inputValidation)
        {
            string contentString;

            switch (inputValidation.Rule)
            {
                case InputValidationRule.Text:
                    if (content is PlainText) return true;
                    break;

                case InputValidationRule.Number:
                    contentString = content.ToString();
                    int result;
                    return int.TryParse(contentString, out result);

                case InputValidationRule.Date:
                    throw new NotSupportedException("Date validation not supported yet");

                case InputValidationRule.Regex:
                    contentString = content.ToString();
                    var regex = new Regex(inputValidation.Regex);
                    return regex.IsMatch(contentString);

                case InputValidationRule.Type:
                    if (content.GetMediaType().Equals(inputValidation.Type)) return true;
                    break;
            }

            return true;
        }
    }

    public class InputSettings
    {
        internal static JsonSerializer Serializer = JsonSerializer.Create(Application.SerializerSettings);

        public DocumentDefinition Label { get; set; }

        public InputValidation Validation { get; set; }

        public string VariableName { get; set; }

        public string Culture { get; set; } = CultureInfo.InvariantCulture.Name;

        public static InputSettings Parse(IDictionary<string, object> dictionary)
            => JObject.FromObject(dictionary).ToObject<InputSettings>(Serializer);

        public void Validate()
        {
            if (Label == null) throw new ArgumentException("Label cannot be null");
            if (VariableName == null) throw new ArgumentException("VariableName cannot be null");
            if (Validation != null)
            {

                if (Validation.Rule == InputValidationRule.Regex
                    && Validation.Regex == null)
                {
                    throw new ArgumentException("Regex validation cannot be null");
                }

                if (Validation.Rule == InputValidationRule.Type
                    && Validation.Type == null)
                {
                    throw new ArgumentException("Type validation cannot be null");
                }
            }

        }
    }

}
