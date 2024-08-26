@PagamentoController
Feature: PagamentoController

   Controller de Pagamento

Scenario: Obtendo tipos de pagamento disponíveis
    Given que o serviço pagamento está configurado
    When eu solicitar os tipos de pagamento
    Then a resposta deve ser 200
    And a resposta deve conter os tipos de pagamento

Scenario: Obtendo pagamento por id do pedido
    Given que o serviço pagamento está configurado
    When eu solicitar o pagamento pelo id do pedido "f694f3a3-2622-45ea-b168-f573f16165ea"
    Then a resposta deve ser 200

Scenario: Recebendo autorização via webhook de pagamento
    Given que o serviço pagamento está configurado
    When o webhook de autorização de pagamento for acionado
    Then a resposta deve ser 204