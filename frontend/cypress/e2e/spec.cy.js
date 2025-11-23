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

const routeCode = 'route-' + faker.string.alphanumeric(5);
const templateCode = 'template-' + faker.string.alphanumeric(5);
const processCode = 'process-' + faker.string.alphanumeric(5);

const logout = () => {
    cy.get('.p-avatar-image').click()
    cy.contains('Выйти').click()
}

const userLogin = (login, password) => {
  cy.get('input[type=text]').clear()
  cy.get('input[type=password]').clear()
  cy.get('input[type=text]').type(login)
  cy.get('input[type=password]').type(password)
  cy.contains('Войти').click()
}

const adminLogin = () => {
    cy.get('input[type=text]').clear()
    cy.get('input[type=password]').clear()
    cy.get('input[type=text]').type('AdministratorDF')
    cy.get('input[type=password]').type('password')
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
    cy.get('.p-avatar-image').click()
    cy.contains('Выйти').click()
    //Рега первого юзера
    cy.contains('Зарегистрироваться').click()
    cy.contains('label', 'Имя пользователя').next('input').type(login1)
    cy.contains('label', 'Пароль').next('input').type(password1)
    cy.contains('label', 'Имя отображаемое').next('input').type(name1)
    cy.contains('label', 'Email').next('input').type(email1)
    cy.contains('Зарегистрироваться').click()
    cy.contains('Регистрация выполнена. Ожидайте подтверждения администратора.')
    cy.visit('http://localhost:5173/login')
    //Рега второго юзера
    cy.contains('Зарегистрироваться').click()
    cy.contains('label', 'Имя пользователя').next('input').type(login2)
    cy.contains('label', 'Пароль').next('input').type(password2)
    cy.contains('label', 'Имя отображаемое').next('input').type(name2)
    cy.contains('label', 'Email').next('input').type(email2)
    cy.contains('Зарегистрироваться').click()
    cy.contains('Регистрация выполнена. Ожидайте подтверждения администратора.')
    cy.visit('http://localhost:5173/login')
    //Рега третьего юзера
    cy.contains('Зарегистрироваться').click()
    cy.contains('label', 'Имя пользователя').next('input').type(login3)
    cy.contains('label', 'Пароль').next('input').type(password3)
    cy.contains('label', 'Имя отображаемое').next('input').type(name3)
    cy.contains('label', 'Email').next('input').type(email3)
    cy.contains('Зарегистрироваться').click()
    cy.contains('Регистрация выполнена. Ожидайте подтверждения администратора.')
    cy.visit('http://localhost:5173/login')
    //Подтверждение юзеров
    cy.get('input[type=text]').type('AdministratorDF')
    cy.get('input[type=password]').type('password')
    cy.contains('Войти').click();
    cy.get('.pi-cog').click();
    cy.contains('Пользователи').click();
    cy.url().should('include', 'admin/users');
    cy.contains('tr', name1).find('button:contains("Подтвердить")').click();
    cy.contains('tr', name2).find('button:contains("Подтвердить")').click();
    cy.contains('tr', name3).find('button:contains("Подтвердить")').click();

    //Создать шаблон
    cy.visit('http://localhost:5173/login')
    adminLogin();
    cy.get('.pi-cog').click();
    cy.contains('Шаблоны').click();
    cy.url().should('include', 'admin/templates');
    cy.get('.pi-plus').click();
    cy.wait(500)
    cy.contains('label', 'Название').next('input').type('Новый шаблон');
    cy.contains('label', 'Код').next('input').type(templateCode)
    cy.contains('label', 'Описание').next('textarea').type('Digital company without paper thanks to AI')
    cy.contains('button', 'Создать').click()
    cy.wait(500)
    cy.get('.p-toast-close-button').click();
    cy.contains('Новый шаблон').click()
    cy.contains('Добавить поле').click()
    cy.wait(500)
    cy.contains('label[for=name]', 'Имя').next('input').type('Наименование поля 1');
    cy.contains('label[for=label]', 'Заголовок').next('input').type('Заголовок поля 1')
    cy.contains('span', 'Текст').click()
    cy.contains('span', 'Текст').click()
    cy.contains('Сохранить').click()
    cy.wait(500)
    cy.get('.p-toast-close-button').click();    
    cy.contains('Добавить поле').click()
    cy.wait(500)
    cy.contains('label[for=name]', 'Имя').next('input').type('Наименование поля 2');
    cy.contains('label[for=label]', 'Заголовок').next('input').type('Заголовок поля 2')
    cy.contains('span', 'Текст').click()
    cy.contains('span', 'Число').click()
    cy.contains('Сохранить').click()
    cy.wait(500)
    cy.get('.p-toast-close-button').click();
    cy.contains('Добавить поле').click()
    cy.wait(500)
    cy.contains('label[for=name]', 'Имя').next('input').type('Наименование поля 3');
    cy.contains('label[for=label]', 'Заголовок').next('input').type('Заголовок поля 3')
    cy.contains('span', 'Текст').click()
    cy.contains('span', 'Дата').click()
    cy.contains('Сохранить').click()
    cy.wait(500)
    cy.get('.p-toast-close-button').click();
    cy.contains('Добавить поле').click()
    cy.wait(500)
    cy.contains('label[for=name]', 'Имя').next('input').type('Наименование поля 4');
    cy.contains('label[for=label]', 'Заголовок').next('input').type('Заголовок поля 4')
    cy.contains('span', 'Текст').click()
    cy.contains('span', 'Список').click()
    cy.get('input[type=checkbox]').click()
    cy.contains('label[for=optionsJson]', 'Варианты (JSON)').next('textarea').clear().type('["1", "2", "3", "4"]')
    cy.contains('button','Сохранить').click()
    cy.get('.p-toast-close-button').click();

    //Создать маршрут
    cy.get('.pi-cog').click();
    cy.contains('Маршруты').click();
    cy.url().should('include', 'admin/routes');
    cy.get('.pi-plus').click();
    cy.wait(500)
    cy.contains('label', 'Название').next('input').type('Новый маршрут');
    cy.contains('label[for=code]', 'Код').next('input').type(routeCode);
    cy.contains('label[for=description]', 'Описание').next('textarea').type('You can export data to various information, inventory, or accounting software.')
    cy.contains('button', 'Создать').click()
    cy.wait(500)
    cy.get('.p-toast-close-button').click();
    cy.contains('Новый маршрут').click()
    cy.contains('Добавить этап').click()
    cy.wait(500)
    cy.contains('label[for=name]', 'Название этапа').next('input').type('Этап номер 1');
    cy.contains('.p-tabview-tab-header', 'Пользователи').find('span:contains("Пользователи")').click()
    cy.contains('label', name1).click();
    cy.get('.p-dialog-content').should('be.visible').scrollTo('bottom');
    cy.get('.p-dialog').find('button:contains("Сохранить")').click();
    cy.wait(500)
    cy.get('.p-toast-close-button').click();
    cy.contains('Добавить этап').click()
    cy.wait(500)
    cy.contains('label[for=name]', 'Название этапа').next('input').type('Этап номер 2');
    cy.contains('.p-tabview-tab-header', 'Пользователи').find('span:contains("Пользователи")').click()
    cy.contains('label', name2).click();
    cy.get('.p-dialog-content').should('be.visible').scrollTo('bottom');
    cy.get('.p-dialog').find('button:contains("Сохранить")').click();
    cy.wait(500)
    cy.get('.p-toast-close-button').click();
    cy.contains('Добавить этап').click()
    cy.wait(500)
    cy.contains('label[for=name]', 'Название этапа').next('input').type('Этап номер 3');
    cy.contains('label', 'Параллельное согласование').click()
    cy.contains('.p-tabview-tab-header', 'Пользователи').find('span:contains("Пользователи")').click()
    cy.contains('label', name3).click();
    cy.get('.p-dialog-content').should('be.visible').scrollTo('bottom');
    cy.get('.p-dialog').find('button:contains("Сохранить")').click();
    cy.get('.p-toast-close-button').click();

    //Создать процесс
    cy.get('.pi-cog').click();
    cy.contains('Процессы').click();
    cy.url().should('include', 'admin/processes');
    cy.contains('button', 'Создать процесс').click()
    cy.wait(500)
    cy.contains('label', 'Название').next('input').type('Новый процесс');
    cy.contains('label', 'Код').next('input').type(processCode)
    cy.contains('label', 'Описание').next('textarea').type('You can issue invoices directly in the system without the need for additional software.')
    cy.contains('span', 'Выберите шаблон').click()
    cy.contains('span', 'Новый шаблон').click()
    cy.contains('span', 'Выберите маршрут').click()
    cy.contains('span', 'Новый маршрут').click()
    cy.get('.p-dialog').find('button:contains("Создать")').click();
    cy.wait(500)
    cy.get('.p-toast-close-button').click();
  })
})