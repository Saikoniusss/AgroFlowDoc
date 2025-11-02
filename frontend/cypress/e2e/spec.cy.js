describe('template spec', () => {
  it('passes', () => {
    cy.visit('http://localhost:5173/application')
    cy.contains('Прием зерна').click()
    cy.url().should('include', '/grain-reception')
    cy.contains('Заявка')
    cy.contains('Счет на оплату расходы на экспорт')
  })
})