import { faker } from '@faker-js/faker';

const login1 = faker.person.firstName();
const name1 = faker.person.fullName();
const password1 = faker.internet.password();
const email1 = faker.internet.email();

const login2 = faker.person.firstName();
const name2 = faker.person.fullName();
const password2 = faker.internet.password();
const email2 = faker.internet.email();

const login3 = faker.person.firstName();
const name3 = faker.person.fullName();
const password3 = faker.internet.password();
const email3 = faker.internet.email();

const logout = () => {
    cy.get('.p-avatar-image').click()
    cy.contains('Выйти').click()
}

const adminLogin = () => {
  cy.get('input[type=text]').clear()
  cy.get('input[type=password]').clear()
  cy.get('input[type=text]').type('AdministratorDF')
  cy.get('input[type=password]').type('password')
  cy.contains('Войти').click()
}

const userLogin = (login, password) => {
  cy.get('input[type=text]').clear()
  cy.get('input[type=password]').clear()
  cy.get('input[type=text]').type(login)
  cy.get('input[type=password]').type(password)
  cy.contains('Войти').click()
}

describe('template spec', () => {
  it('passes', () => {
    //Вход админа
    cy.visit('http://localhost:5173/login')
    cy.get('input[type=text]').type('fake@email.com')
    cy.get('input[type=password]').type('fake@email.com')
    cy.contains('Войти').click()
    cy.contains('Неверный логин или пароль')
    cy.get('input[type=text]').clear()
    cy.get('input[type=password]').clear()
    cy.get('input[type=text]').type('AdministratorDF')
    cy.get('input[type=password]').type('password')
    cy.contains('Войти').click()
    // cy.get('.p-avatar-image').click()
    // cy.contains('Выйти').click()
    // //Рега первого юзера
    // cy.contains('Зарегистрироваться').click()
    // cy.contains('label', 'Имя пользователя').next('input').type(login1)
    // cy.contains('label', 'Пароль').next('input').type(password1)
    // cy.contains('label', 'Имя отображаемое').next('input').type(name1)
    // cy.contains('label', 'Email').next('input').type(email1)
    // cy.contains('Зарегистрироваться').click()
    // cy.contains('Регистрация выполнена. Ожидайте подтверждения администратора.')
    // cy.visit('http://localhost:5173/login')
    // //Рега второго юзера
    // cy.contains('Зарегистрироваться').click()
    // cy.contains('label', 'Имя пользователя').next('input').type(login2)
    // cy.contains('label', 'Пароль').next('input').type(password2)
    // cy.contains('label', 'Имя отображаемое').next('input').type(name2)
    // cy.contains('label', 'Email').next('input').type(email2)
    // cy.contains('Зарегистрироваться').click()
    // cy.contains('Регистрация выполнена. Ожидайте подтверждения администратора.')
    // cy.visit('http://localhost:5173/login')
    // //Рега третьего юзера
    // cy.contains('Зарегистрироваться').click()
    // cy.contains('label', 'Имя пользователя').next('input').type(login3)
    // cy.contains('label', 'Пароль').next('input').type(password3)
    // cy.contains('label', 'Имя отображаемое').next('input').type(name3)
    // cy.contains('label', 'Email').next('input').type(email3)
    // cy.contains('Зарегистрироваться').click()
    // cy.contains('Регистрация выполнена. Ожидайте подтверждения администратора.')
    // cy.visit('http://localhost:5173/login')
    // //Подтверждение юзеров
    // cy.get('input[type=text]').type('AdministratorDF')
    // cy.get('input[type=password]').type('password')
    // cy.contains('Войти').click();
    // cy.get('.pi-cog').click();
    // cy.contains('Пользователи').click();
    // cy.url().should('include', 'admin/users');
    // cy.contains('tr', name1).find('button:contains("Подтвердить")').click();
    // cy.contains('tr', name2).find('button:contains("Подтвердить")').click();
    // cy.contains('tr', name3).find('button:contains("Подтвердить")').click();
    // logout();
    // //Создать шаблон
    // adminLogin();
    cy.get('.pi-cog').click();
    cy.contains('Шаблоны').click();
    cy.url().should('include', 'admin/templates');
    cy.get('.pi-plus').click();
    cy.wait(100)
    cy.contains('label', 'Название').next('input').type('Новый шаблон');
    cy.contains('label', 'Код').next('input').type('3293479')
    cy.contains('label', 'Описание').next('textarea').type('Digital company without paper thanks to AI')
    cy.contains('button', 'Создать').click()
    cy.wait(500)
    cy.contains('Новый шаблон').click()
    cy.contains('Добавить поле').click()
    cy.wait(500)
    cy.contains('label[for=name]', 'Имя').next('input').type('Наименование поля 1');
    cy.contains('label[for=label]', 'Заголовок').next('input').type('Заголовок поля 1')
    cy.contains('span', 'Текст').click()
    cy.contains('span', 'Текст').click()
    cy.contains('Сохранить').click()
    cy.wait(500)
    cy.contains('Добавить поле').click()
    cy.wait(500)
    cy.contains('label[for=name]', 'Имя').next('input').type('Наименование поля 2');
    cy.contains('label[for=label]', 'Заголовок').next('input').type('Заголовок поля 2')
    cy.contains('span', 'Текст').click()
    cy.contains('span', 'Число').click()
    cy.contains('Сохранить').click()
    cy.wait(500)
    cy.contains('Добавить поле').click()
    cy.wait(500)
    cy.contains('label[for=name]', 'Имя').next('input').type('Наименование поля 3');
    cy.contains('label[for=label]', 'Заголовок').next('input').type('Заголовок поля 3')
    cy.contains('span', 'Текст').click()
    cy.contains('span', 'Дата').click()
    cy.contains('Сохранить').click()
    cy.wait(500)
    cy.contains('Добавить поле').click()
    cy.wait(500)
    cy.contains('label[for=name]', 'Имя').next('input').type('Наименование поля 4');
    cy.contains('label[for=label]', 'Заголовок').next('input').type('Заголовок поля 4')
    cy.contains('span', 'Текст').click()
    cy.contains('span', 'Список').click()
    cy.get('input[type=checkbox]').click()
    cy.contains('label[for=optionsJson]', 'Варианты (JSON)').next('textarea').clear().type('["1", "2", "3", "4"]')
    cy.contains('button','Сохранить').click()
  })
})