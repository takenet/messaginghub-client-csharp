**Infraestrutura de Mensagens do Messaging Hub**

O Messaging Hub possui uma infraestrutura que permite que os Chat Bots sejam construídos usando uma linguagem canônica, que é devidamente traduzida para as mensagens específicas de cada um dos canais disponíveis, como Facebook Messenger, Skype, SMS.

**Os seguintes tipos canônicos estão disponíveis:**

- **PlainText:**
  Este é o tipo de mensagem padrão e é utilizado para o envio de mensagens de texto simples.
- **MediaLink:**
  O tipo MediaLink é usado para enviar imagens. Em canais que não suportam mensagens um link para um endereçoi web contendo a imagem é enviado.
- **WebLink:**
  Um WebLink pode ser usado para enviar links para paginas web. Alguns canais, como o OMNI e o Facebook, fazem um excelente tratamento desse tipo, exibindo uma miniatura da página dentro da própria thread de mensagens.
- **Select:**
  Um Select permite o envio ao cliente do Chat Bot de uma lista de opções, da qual ele pode selecionar uma delas como resposta.
- **Location:**
  O tipo Location pode ser usado pelo canal para enviar ao Chat Bot a localização geográfica do cliente.
- **Invoice:**
  O tipo Invoice pode ser usado pelo Chat Bot para solicitar um pagamento a um canal de pagamento, como por exemplo o PagSeguro.
- **InvoiceStatus:**
  InvoiceStatus são mensagens recebidas pelo Chat Bot, a partir do canal de pagamento, comunicando o status do pagamento solicitado.
- **PaymentReceipt:**
  Um PaymentReceipt é o tipo de mensagem que deve ser enviado ao cliente que realizou um pagamento.
- **DocumentCollection:**
  DocumentCollections permitem que múltiplas mensagens sejam enviadas dentro de uma única mensagem.
- **DocumentContainer:**
  Encapsula um conteúdo de forma a ser utilizado junto ao DocumentCollection para envio de multiplos conteúdos diferentes. Util para mandar conteúdos compostos (texto + imagem).
