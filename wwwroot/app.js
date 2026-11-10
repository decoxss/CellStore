const contenedor = document.getElementById('productos');
const estado = document.getElementById('estado');
const listaCarrito = document.getElementById('lista-carrito');
const totalTexto = document.getElementById('total');
const cantidadTexto = document.getElementById('cantidad-carrito');
const panel = document.getElementById('panel-carrito');
const fondo = document.getElementById('fondo-carrito');
const mensajeCarrito = document.getElementById('mensaje-carrito');

let celulares = [];

// el carrito guarda solo id y cantidad; precio y stock se leen del catálogo actual
let carrito = [];
try {
    carrito = JSON.parse(localStorage.getItem('carrito')) || [];
} catch {
    carrito = [];
}

function escapar(texto) {
    return String(texto).replace(/[&<>"']/g, caracter => ({
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#39;'
    })[caracter]);
}

function obtenerImagen(celular) {
    const marca = String(celular.marca).toLowerCase();
    const modelo = String(celular.modelo).toLowerCase();

    if (marca.includes('samsung')) {
        return 'img/samsung.jpg';
    }

    if (marca.includes('apple') || modelo.includes('iphone')) {
        return 'img/iphone.jpg';
    }

    if (marca.includes('xiaomi')) {
        return 'img/xiaomi.jpg';
    }

    if (marca.includes('motorola')) {
        return 'img/motorola.jpg';
    }

    return 'img/samsung.jpg';
}

async function cargarCelulares() {
    try {
        const respuesta = await fetch('/api/celulares');

        if (!respuesta.ok) {
            throw new Error('No se pudieron cargar los celulares');
        }

        const datos = await respuesta.json();
        celulares = datos.map(c => ({
            ...c,
            id: Number(c.id),
            precio: Number(c.precio),
            stock: Number(c.stock)
        }));

        estado.textContent = celulares.length
            ? ''
            : 'No hay celulares registrados.';

        mostrarCarrito();   // también vuelve a pintar las tarjetas
    } catch (error) {
        estado.textContent = 'No se pudieron cargar los celulares. Revisa el servidor.';
        console.error(error);
    }
}

function disponibles(celular) {
    const enCarrito = carrito.find(item => item.id === celular.id);
    return celular.stock - (enCarrito ? enCarrito.cantidad : 0);
}

function mostrarCelulares() {
    contenedor.innerHTML = celulares.map(celular => `
        <article class="producto">
            <div class="imagen">
                <img
                    src="${obtenerImagen(celular)}"
                    alt="${escapar(celular.marca)} ${escapar(celular.modelo)}"
                    onerror="this.style.display='none'"
                >
            </div>

            <div class="contenido">
                <span class="marca">${escapar(celular.marca)}</span>
                <h3>${escapar(celular.modelo)}</h3>

                <p class="precio">
                    $${Number(celular.precio).toLocaleString('es-MX', {
                        minimumFractionDigits: 2
                    })}
                </p>

                <p class="stock">${disponibles(celular) > 0 ? disponibles(celular) + ' disponibles' : 'Agotado'}</p>

                <button class="boton"
                    onclick="agregarAlCarrito(${Number(celular.id)})"
                    ${disponibles(celular) <= 0 ? 'disabled' : ''}>
                    ${disponibles(celular) > 0 ? 'Agregar al carrito' : 'Sin existencias'}
                </button>
            </div>
        </article>
    `).join('');
}

const formatoPesos = n =>
    '$' + Number(n).toLocaleString('es-MX', { minimumFractionDigits: 2 });

function guardarCarrito() {
    localStorage.setItem('carrito', JSON.stringify(carrito));
}

function abrirCarrito() {
    panel.classList.add('abierto');
    fondo.classList.add('visible');
}

function cerrarCarrito() {
    panel.classList.remove('abierto');
    fondo.classList.remove('visible');
}

function agregarAlCarrito(id) {
    const celular = celulares.find(item => item.id === id);
    if (!celular) return;

    const elemento = carrito.find(item => item.id === id);

    if (elemento) {
        if (elemento.cantidad >= celular.stock) {
            alert('No hay más existencias disponibles.');
            return;
        }
        elemento.cantidad++;
    } else {
        carrito.push({ id, cantidad: 1 });
    }

    mostrarCarrito();
    abrirCarrito();
}

function cambiarCantidad(id, cambio) {
    const elemento = carrito.find(item => item.id === id);
    const celular = celulares.find(item => item.id === id);
    if (!elemento || !celular) return;

    const nueva = elemento.cantidad + cambio;

    if (nueva <= 0) return quitarDelCarrito(id);
    if (nueva > celular.stock) return;

    elemento.cantidad = nueva;
    mostrarCarrito();
}

function quitarDelCarrito(id) {
    carrito = carrito.filter(item => item.id !== id);
    mostrarCarrito();
}

function vaciarCarrito() {
    carrito = [];
    mostrarCarrito();
}

function mostrarCarrito() {
    // quita productos que ya no existen y ajusta cantidades al stock actual
    if (celulares.length) {
        carrito = carrito
            .filter(item => celulares.some(c => c.id === item.id))
            .map(item => {
                const c = celulares.find(c => c.id === item.id);
                return { id: item.id, cantidad: Math.min(item.cantidad, c.stock) };
            })
            .filter(item => item.cantidad > 0);
    }

    guardarCarrito();

    const lineas = carrito.map(item => {
        const c = celulares.find(c => c.id === item.id);
        return c ? { ...c, cantidad: item.cantidad } : null;
    }).filter(Boolean);

    listaCarrito.innerHTML = lineas.length
        ? lineas.map(item => `
            <div class="item-carrito">
                <div class="item-miniatura">
                    <img src="${obtenerImagen(item)}" alt="" onerror="this.style.display='none'">
                </div>

                <div class="item-datos">
                    <strong>${escapar(item.marca)} ${escapar(item.modelo)}</strong>
                    <span class="item-precio">${formatoPesos(item.precio)} c/u</span>

                    <div class="cantidad">
                        <button onclick="cambiarCantidad(${Number(item.id)}, -1)">−</button>
                        <span>${item.cantidad}</span>
                        <button onclick="cambiarCantidad(${Number(item.id)}, 1)"
                            ${item.cantidad >= item.stock ? 'disabled' : ''}>+</button>
                    </div>
                </div>

                <div class="item-derecha">
                    <strong>${formatoPesos(Number(item.precio) * item.cantidad)}</strong>
                    <button class="quitar" onclick="quitarDelCarrito(${Number(item.id)})">Quitar</button>
                </div>
            </div>
        `).join('')
        : '<p class="carrito-vacio">Tu carrito está vacío.</p>';

    cantidadTexto.textContent = lineas.reduce((s, i) => s + i.cantidad, 0);
    totalTexto.textContent = formatoPesos(
        lineas.reduce((s, i) => s + Number(i.precio) * i.cantidad, 0)
    ) + ' MXN';

    document.getElementById('confirmar').disabled = lineas.length === 0;
    document.getElementById('vaciar').style.display = lineas.length ? 'block' : 'none';

    mostrarCelulares();   // actualiza los "disponibles" de las tarjetas
}

async function confirmarCompra() {
    mensajeCarrito.textContent = '';

    const sesion = await fetch('/api/auth/sesion');
    if (!sesion.ok) {
        mensajeCarrito.textContent = 'Inicia sesión para confirmar tu compra.';
        setTimeout(() => location.href = 'login.html', 1200);
        return;
    }

    const boton = document.getElementById('confirmar');
    boton.disabled = true;

    try {
        const res = await fetch('/api/pedidos', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ items: carrito })
        });
        const data = await res.json();

        if (!res.ok) {
            mensajeCarrito.textContent = data.error;
            await cargarCelulares();   // refresca stock por si cambió
            return;
        }

        carrito = [];
        await cargarCelulares();       // muestra el stock ya descontado
        mensajeCarrito.innerHTML =
            `¡Compra realizada! Pedido #${Number(data.id)} por ${formatoPesos(data.total)} MXN.<br>
             <a href="ticket.html?id=${Number(data.id)}" target="_blank">Ver ticket</a>`;
    } catch (error) {
        mensajeCarrito.textContent = 'No se pudo completar la compra.';
        console.error(error);
    } finally {
        mostrarCarrito();   // reactiva o desactiva el botón según el carrito
    }
}

async function cargarSesion() {
    const menu = document.getElementById('menu-sesion');

    function enlace(texto, href, alClic) {
        const a = document.createElement('a');
        a.textContent = texto;
        a.href = href || '#';
        if (alClic) a.addEventListener('click', alClic);
        return a;
    }

    try {
        const res = await fetch('/api/auth/sesion');
        if (!res.ok) throw new Error('sin sesion');
        const usuario = await res.json();

        const saludo = document.createElement('span');
        saludo.className = 'saludo';
        saludo.textContent = 'Hola, ' + usuario.nombre;
        menu.append(saludo);

        if (usuario.rol === 'admin') {
            menu.append(enlace('Panel admin', 'admin.html'));
            menu.append(enlace('Dashboard', 'dashboard.html'));
        }

        menu.append(enlace('Cerrar sesión', '#', async e => {
            e.preventDefault();
            await fetch('/api/auth/logout', { method: 'POST' });
            location.reload();
        }));
    } catch {
        menu.append(
            enlace('Iniciar sesión', 'login.html'),
            enlace('Registrarme', 'registro.html')
        );
    }
}

document.getElementById('abrir-carrito').addEventListener('click', e => {
    e.preventDefault();
    abrirCarrito();
});
document.getElementById('cerrar-carrito').addEventListener('click', cerrarCarrito);
fondo.addEventListener('click', cerrarCarrito);
document.getElementById('vaciar').addEventListener('click', vaciarCarrito);
document.getElementById('confirmar').addEventListener('click', confirmarCompra);
document.addEventListener('keydown', e => { if (e.key === 'Escape') cerrarCarrito(); });

cargarSesion();
cargarCelulares();