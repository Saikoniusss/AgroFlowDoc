<script setup>
import { ref } from 'vue';
import DataView from "primevue/dataview"
import Divider from 'primevue/divider';

const documents = ref([
    {
        id: 1,
        title: 'Заявка',
        date: '2024-01-01',
        number: '110/12345',
        image: 'https://randomuser.me/api/portraits/women/4.jpg',
        description: 'Описание заявки'
    },
    {
        id: 2,
        title: 'Счет на оплату',
        date: '2024-02-15',
        image: 'https://randomuser.me/api/portraits/women/15.jpg',
        number: '110/67890',
        description: 'Описание счета на оплату'
    },
    {
        id: 3,
        title: 'Счет на оплату расходы на экспорт',
        date: '2024-03-30',
        number: '110/11223',
        image: 'https://randomuser.me/api/portraits/men/7.jpg',
        description: 'Описание счета на оплату расходы на экспорт'
    },
    {
        id: 4,
        title: 'Счет на оплату',
        date: '2024-02-15',
        image: 'https://randomuser.me/api/portraits/women/12.jpg',
        number: '110/67820',
        description: 'Описание счета на оплату'
    },
    {
        id: 5,
        title: 'Счет на оплату расходы на экспорт',
        date: '2024-03-30',
        number: '110/11283',
        image: 'https://randomuser.me/api/portraits/men/17.jpg',
        description: 'Описание счета на оплату расходы на экспорт'
    },
    {
        id: 6,
        title: 'Счет на оплату расходы на экспорт',
        date: '2024-03-30',
        number: '110/11203',
        image: 'https://randomuser.me/api/portraits/men/20.jpg',
        description: 'Описание счета на оплату расходы на экспорт'
    }
]);

</script>

<template>
    <DataView :value="documents" layout="list" dataKey="id" paginator :rows="4">
        <template #list="slotProps">
            <div class="flex flex-column gap-3" style="background-color: #d7cfcf;">
                <div v-for="item in slotProps.items" :key="item.id" class="p-3 border-1 surface-border surface-0 border-round grid text-left mr-0 ml-0">
                    <div class="w-1">
                        <img :src="item.image" alt="Document Image" class="w-6 border-round">
                    </div>
                    <div class="w-8 grid">
                        <div class="text-xl font-bold w-8">{{ item.title }}</div>
                        <div class="font-bold w-4">№ {{ item.number }} от {{ item.date }}</div>
                        <Divider class="my-1" />
                        <div class="text-md w-8">Отправитель</div>
                        <div class="w-4">Иванов И.И.</div>
                        <Divider class="my-1" />
                        <div class="text-md w-8">Краткое содержание</div>
                        <div class="w-4">{{ item.description }}</div>
                        <Divider class="my-1" />
                        <div class="text-md w-12">Контрольный срок: Нет</div>
                    </div>
                    <div class="w-3 text-right">
                        <span class="pi pi-envelope m-1"></span>
                        <span class="pi pi-folder-open m-1"></span>
                    </div>
                </div>
            </div>
        </template>
    </DataView>
</template>

<style scoped>

</style>